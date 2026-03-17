<?php

class eResMission
{
    const OK = 0;
    const NOT_IN_SEASON = 1;
    const NO_REWARD = 2;
    const ALREADY_REWARDED = 3;
    const NOT_COMPLETED = 4;
    const NO_TICKET = 5;
    const MAX_LEVEL = 6;
    const INVALID_PARAM = 7;
    const INVALID_STEP = 8;
}

function receive_pass_rewards(int $level = 0, int $step = 0) : array {    
    if (0 !== $step) {
        return receive_one_pass_reward($level, $step);
    }

    $pass = Pass::get();

    $ret = [];
    if ($level) {
        $ret['level'] = $level;
        
        // handle error
        if ($pass->get_level() <= $level) {
            $ret['rs'] = eResMission::NOT_COMPLETED;
            return $ret;
        }
    }

    $raw_list = $pass->get_finished_list($level);

    // filter memories
    $reward_list = [];
    foreach ($raw_list as $item) {
        $rew_info = PassData::get_reward_info($item[0], $item[1]);
        if (!$rew_info || !($gd = new Good($rew_info))) {
            continue;            
        }

        if (eGoodsType::MEMORY !== $gd->type) {
            $reward_list[] = $item;
        }
    }


    if (empty($reward_list)) {
        $ret['rs'] = ($level) ?
            eResMission::ALREADY_REWARDED :
            eResMission::NO_REWARD;
        return $ret;
    }

    foreach ($reward_list as $item) {
        $pass->check_rewarded($item[0], $item[1]);
    }
    $pass->save_sql();

    $ret['rewarded'] = $pass->get_rewarded_array();
    $rew = build_reward_list($reward_list);
    if (!empty($rew)) {
        $ret['rew'] = User::receive_rewards($rew, Usage::BATTLE_PASS);
        User::SaveBaseInfo();
    }

    $ret['rs'] = eResMission::OK;
    return $ret;
}

function receive_one_pass_reward(int $level, int $step) : array {
    $pass = Pass::get();

    $ret = [];
    $ret['level'] = $level;
    $ret['step'] = $step;

    if ($pass->get_level() < $level) {
        Response::api_error(eResMission::NOT_COMPLETED, $ret);
    } else if ($pass->get_step() < $step) {
        Response::api_error(eResMission::INVALID_STEP, $ret);
    } else if (!$pass->has_reward($level, $step)) {
        Response::api_error(eResMission::ALREADY_REWARDED, $ret);
    }

    $pass->check_rewarded($level, $step);
    $pass->save_sql();

    $ret['rewarded'] = $pass->get_rewarded_array();
    $rew = build_reward_list([[$level, $step]]);
    if (empty($rew)) {
        $ret['rs'] = eResMission::NO_REWARD;
    } else {
        $ret['rs'] = eResMission::OK;
        $ret['rew'] = User::receive_rewards($rew, Usage::BATTLE_PASS);
    }

    return $ret;
}

function build_reward_list(array $lvl_step) : array {
    $gold = 0;
    $item = [];
    $point = 0;
    $memory = [];

    foreach ($lvl_step as $i) {
        $rew = PassData::get_reward_info($i[0], $i[1]);
        if (!$rew || !is_array($rew) || !isset($rew[2])) {
            Response::error(ResCode::SERVER_ERROR, 'Pass reward data error for ' . json_encode($i));
        }

        switch ($rew[0]) {
            case 'gold':
                $gold += $rew[2];
                break;
            case 'item':
                $ino = $rew[1];
                $cnt = $rew[2];
                if (!isset($item[$ino])) {
                    $item[$ino] = 0;
                }
                $item[$ino] += $cnt;
                break;
            case 'point':
                $point += $rew[2];
                break;
            case 'memory':
                $memory[] = [$rew[1], NECO_MEMORY_DUPE_POINT_PREMIUM];
                break;
            default:
            // warning
                Response::error(ResCode::SERVER_ERROR, 'cannot parse pass reward type ' . $rew[0]);
                break;
        }
    }

    $rew = [];
    if ($gold) {
        $rew['gold'] = $gold;
    }
    if ($point) {
        $rew['point'] = $point;        
    }
    if (!empty($item)) {
        $rew['item'] = [];
        foreach ($item as $id => $cnt) {
            $rew['item'][] = ['id' => intval($id), 'amount' => $cnt];
        }
    }
    if (!empty($memory)) {
        sort($memory);
        $rew['memory'] = $memory;
    }

    return $rew;
}

function use_pass_ticket() : array {
    $ret = ['rs' => eResMission::OK];

    $pass = Pass::get();
    if (PassData::get_max_level() <= $pass->get_level()) {
        $ret['rs'] = eResMission::MAX_LEVEL;
        return $ret;
    }

    $exp = PassData::get_exp_interval($pass->get_level());
    if (!$exp) {
        Response::error(ResCode::SERVER_ERROR, 'pass level-exp interval invalid for level ' . $pass->get_level());
    }

    $inven = User::GetInventory();
    if (0 >= $inven->get_item_amount(ITEM_PASS_TICKET)) {
        $ret['rs'] = eResMission::NO_TICKET;
        return $ret;
    }

    $inven->update_item_amount(ITEM_PASS_TICKET, -1, Usage::BATTLE_PASS);
    Pass::add_exp($exp);

    return $ret;
}

// EOF
