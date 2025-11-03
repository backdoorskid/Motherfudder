use rand::Rng;

use super::{pspace, rand_between, random_boolean_with_probability, random_casing, random_variable};

static DONT_SPLIT_CHARS: [char; 3] = ['^', '%', '~'];

fn escape_line(line: &String) -> String {
    let mut result = String::new();
    let line_chars = line.chars();

    for c in line_chars {
        if c == '^' {
            result.push('^');
            result.push('^');
        }
        result.push(c);
    }

    result
}

fn add_junk_vars_into_part(part: String) -> String {
    if part.len() < 3 {
        return part;
    }

    let mut result = String::new();
    let part_chars = part.chars();
    let mut cur_max_junk_calls = rand_between(1, part.len() as u32 / 2);
    let mut in_var = false;

    for c in part_chars {
        result.push(c);
        if c == '%' {in_var = !in_var}
        if random_boolean_with_probability(0.3) && cur_max_junk_calls != 0 && !DONT_SPLIT_CHARS.contains(&c) && !in_var {
            result += &format!("%{}%", random_variable());
            cur_max_junk_calls -= 1;
        }
    }

    result
}

fn split_line(line: String) -> Vec<String> {
    let mut result = Vec::new();
    let mut remaining = line;
    while !remaining.is_empty() {
        let mut len = match remaining.len() {
            n if n < 3 => {
                let piece = remaining.clone();
                result.push(piece);
                break;
            }
            q => {
                let h = if q > 7 {7} else {q};
                rand::thread_rng().gen_range(3..=h)
            }
        };
        let mut try_increase: bool = true;
        while DONT_SPLIT_CHARS.contains(&(remaining.as_bytes()[len - 1] as char)) {
            if try_increase {
                len += 1;
                if len >= remaining.len() {
                    try_increase = false;
                    len -= 1;
                }
            } else {
                len -= 1;
            }
        }
        result.push(remaining[..len].to_string());
        remaining = remaining[len..].to_string();
    }
    
    result
}

pub fn create_joined_def_lines(variable_defs: Vec<String>) -> String {
    let mut joined_defs = String::new();
    let mut is_line_beginning = true;
    let mut to_do_on_line = rand_between(1, 4);

    for var_def in variable_defs {
        if !is_line_beginning {
            joined_defs += "&&";
            joined_defs += pspace();
        }
        joined_defs += &var_def;
        if is_line_beginning {
            is_line_beginning = false;
        }
        to_do_on_line -= 1;
        if to_do_on_line == 0 {
            is_line_beginning = true;
            to_do_on_line = rand_between(1, 4);
            joined_defs += "\n";
        }
    }

    if !joined_defs.ends_with("\n") {
        joined_defs += "\n";
    }

    joined_defs
}

pub fn apply(line: &String) -> String {
    let line_escaped = escape_line(line);
    let line_escaped_split = split_line(line_escaped);

    let mut variable_call = String::new();
    let mut variable_defs: Vec<String> = Vec::new();
    for part in line_escaped_split {
        let new_variable = random_variable();
        variable_call += &format!("%{}%", new_variable.clone());
        variable_defs.push(format!("{} {}={}", random_casing("set"), new_variable, add_junk_vars_into_part(part)));
    }

    create_joined_def_lines(variable_defs) + &variable_call + "\n"
    //line.clone() + "\n"
}