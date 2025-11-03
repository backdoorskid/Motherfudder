pub struct RC4 {
    s: [u8; 256],
    i: usize,
    j: usize,
}

impl RC4 {
    pub fn new(key: &[u8]) -> Self {
        let mut s = [0u8; 256];
        let mut j = 0;

        for i in 0..256 {
            s[i] = i as u8;
        }

        for i in 0..256 {
            j = (j + s[i] as usize + key[i % key.len()] as usize) % 256;
            s.swap(i, j);
        }

        RC4 { s, i: 0, j: 0 }
    }

    fn next_byte(&mut self) -> u8 {
        self.i = (self.i + 1) % 256;
        self.j = (self.j + self.s[self.i] as usize) % 256;
        self.s.swap(self.i, self.j);
        let t = (self.s[self.i] as usize + self.s[self.j] as usize) % 256;
        self.s[t]
    }

    pub fn cipher(&mut self, data: &mut Vec<u8>) {
        for i in 0..data.len() {
            data[i] ^= self.next_byte();
        }
    }
}