use sha2::{Digest, Sha256};

pub fn derive_key(starting_state: &Vec<u8>, seed: u8) -> Vec<u8> {
    let mut state = starting_state.clone();
    for i in 0..32 { state[i] ^= seed; }

    let mut hasher = Sha256::new();
    hasher.update(&state);
    hasher.finalize().to_vec()
}