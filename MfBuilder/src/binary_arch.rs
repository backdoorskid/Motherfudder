use goblin::Object;

#[derive(PartialEq, Debug)]
pub enum BinaryArch {
    X64,
    X86,
    NET64,
    NET86,
    Unknown
}

impl BinaryArch {
    pub fn determine(file: &Vec<u8>) -> BinaryArch {
        match goblin::Object::parse(&file) {
            Ok(Object::PE(pe)) => {
                let mut is_dot_net = false;

                if let Some(opt) = pe.header.optional_header {
                    is_dot_net = opt.data_directories.get_clr_runtime_header().is_some();
                }

                if pe.is_64 {
                    if is_dot_net {BinaryArch::NET64} else {BinaryArch::X64}
                } else {
                    if is_dot_net {BinaryArch::NET86} else {BinaryArch::X86}
                } 
            }
            _ => {BinaryArch::Unknown}
        }
    }
}