<?php

// enum op chore
class OpChore
{
    const DO_WORK = 1;
}

// response code
class ResChore
{
    const OK = 0;
}

function get_chore_reward() {
    // if (1 >= User::get_level()) {
    //     return ['exp' => mt_rand(9, 12)];
    // } else if (0 === mt_rand(0, 9)) {
    //     return ['exp' => mt_rand(2,4)];
    // } else {
        return ['gold' => mt_rand(3, 5)];
    // }
}

function do_work() {
    $ret = [];

    $rew = get_chore_reward();
    if (!empty($rew)) {
        $ret['rew'] = $rew;
    }

    $ret['rs'] = ResChore::OK;

    return $ret;
}

function is_chore_moderate(array $rew) : bool {
    if (isset($rew['gold'])) {
        $limit = 10;
        $plant = Plant::get(ePlantType::FISH_FARM);
        if ($plant) {
            $limit = $plant->get_level() * 5;
        }
        if ($limit < $rew['gold']) {
            return false;
        }
    } else if (isset($rew['point']) && 5 < $rew['point']) {
        return false;
    } else if (isset($rew['catnip']) && 5 < $rew['catnip']) {
        return false;
    }

    if (isset($rew['item'])) {
        $limit = 1;
        $basket = Plant::get(ePlantType::GIFT_BASKET);
        if ($basket) {
            $limit = $basket->get_level() * 5;
        }
        if ($limit < count($rew['item'])) {
            return false;
        }

        foreach ($rew['item'] as $item) {
            if ($limit < $item['amount']) {
                return false;
            }
        }
    }

    return true;
}

// EOF
