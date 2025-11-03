use des::{cipher::{generic_array::GenericArray, BlockEncrypt, KeyInit}, TdesEde2};

pub fn encrypt_data(data: Vec<u8>, key: Vec<u8>) -> Vec<u8> {
    let mut padded_data = data.clone();

    let i = 8 - data.len() as u8 % 8;
    for _ in 0..i { padded_data.push(i); }

    let blocks: Vec<&[u8]> = padded_data.chunks(8).collect();
    let mut current_block = [0; 8];
    let mut result: Vec<u8> = Vec::new();

    let mut key_list = [0; 16];
    for i in 0..16 { key_list[i] = key[i]; }
    let key = GenericArray::from(key_list);
    let cipher = TdesEde2::new(&key);

    for block in blocks {
        for i in 0..8 { current_block[i] = block[i]; }
        let mut cb = GenericArray::from(current_block);
        cipher.encrypt_block(&mut cb);
        result.extend_from_slice(cb.as_slice());
    }

    result
}