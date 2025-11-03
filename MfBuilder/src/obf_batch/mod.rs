#![allow(static_mut_refs)]
use rand::{distributions::Alphanumeric, rngs::OsRng, Rng, RngCore};

pub mod obf_pseudo_batcloak;

static mut VARIABLE_CHECK_LIST: Vec<String> = Vec::new();

fn rand_between(lower: u32, upper: u32) -> u32 {
    return lower + (OsRng.next_u32() % (upper - lower + 1));
}

fn random_string(length: u32) -> String {
    return OsRng
        .sample_iter(&Alphanumeric)
        .take(length as usize)
        .map(char::from)
        .collect();
}

fn random_boolean_with_probability(prob: f64) -> bool {
    rand::Rng::gen_bool(&mut OsRng, prob)
}

pub fn random_variable() -> String {
    unsafe {
        loop {
            let variable = random_string(rand_between(4, 8));
            if !VARIABLE_CHECK_LIST.contains(&variable) {
                if !"0123456789".contains(variable.chars().nth(0).unwrap()) {
                    VARIABLE_CHECK_LIST.push(variable.clone());
                    return variable;
                }
            }
        }
    }
}

pub fn random_casing(string: &str) -> String {
    let mut out = String::new();
    let chars = string.chars();
    for i in chars {
        let mut nth_char = i.clone().to_string();
        if random_boolean_with_probability(0.5) {
            nth_char = nth_char.to_uppercase();
        } else {
            nth_char = nth_char.to_lowercase();
        }
        out += &nth_char;
    }
    out
}

pub fn pspace() -> &'static str {
    if random_boolean_with_probability(0.5) {
        " "
    } else {
        ""
    }
}