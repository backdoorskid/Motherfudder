use std::iter;

use rand::{rngs::OsRng, Rng};
use uuid::Uuid;

pub fn guid() -> String {
    Uuid::new_v4().to_string()
}

pub fn generate(len: usize) -> String {
    const CHARSET: &[u8] = b"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    let one_char = || CHARSET[OsRng.gen_range(0..CHARSET.len())] as char;
    iter::repeat_with(one_char).take(len).collect()
}