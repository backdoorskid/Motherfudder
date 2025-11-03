use std::path::Path;
use std::{io, fs};

use colored::{Color, Colorize};

use crate::h;

pub fn copy_dir_all(src: impl AsRef<Path>, dst: impl AsRef<Path>) -> io::Result<()> {
    fs::create_dir_all(&dst)?;
    for entry in fs::read_dir(src)? {
        let entry = entry?;
        let ty = entry.file_type()?;
        if ty.is_dir() {
            copy_dir_all(entry.path(), dst.as_ref().join(entry.file_name()))?;
        } else {
            fs::copy(entry.path(), dst.as_ref().join(entry.file_name()))?;
        }
    }
    Ok(())
}

pub fn remove_dir_all<P: AsRef<Path>>(p: P, s: &str) -> io::Result<()> {
    for entry in fs::read_dir(p)? {
        let entry = entry?;
        let path = entry.path();

        if entry.file_type()?.is_dir() {
            remove_dir_all(&path, s)?;
            fs::remove_dir(&path)?;
            println!("{}{}{}", h(), "Folder deleted:      ".color(Color::Yellow), path.to_str().unwrap().to_string().replace(s, "WorkingDirectory").color(Color::Blue));
        
        } else {
            fs::remove_file(&path)?;
            println!("{}{}{}", h(), "File deleted:        ".color(Color::Yellow), path.to_str().unwrap().to_string().replace(s, "WorkingDirectory").color(Color::Blue));
        }
    }
    Ok(())
}