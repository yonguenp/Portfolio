<?php

class ResCard
{
    const OK                = 0;
    const INVALID_UID       = 1;
    const CANNOT_OVERWRITE  = 2;
    const INVALID_MEMO      = 3;
    const NOT_ENOUGH_COST   = 4;
    const FULLY_EXPANDED    = 5;
}

function is_card_memo_usable(string $memo) {
    // encoding
    if (!is_utf8($memo)) {
        return false;
    }
    // length
    if (Card::MEMO_MAX_LEN < mb_strlen($memo, 'utf8')) {
        return false;
    }

    // censor
    $bonds = [' ', '-', '_', '1', '`', '\''];
    if (preg_match(require CONFIG_PATH . 'config-badwords.php',
        $memo,//str_replace($bonds, '', $memo),
        $matches)) {
        foreach($matches as $word) {
            if (!empty($word)) {
                return false;
            }
        }
    }

    return true;
}

function get_deck_expansion_cost(int $level) : int {
    if (0 >= $level) {
        return 0;
    }

    return 5;
    //return 10 + 10 * $level * $level;
}

// EOF
