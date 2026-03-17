<?php

class OpCook
{
    const COOK = 1;
}

class OpCraft
{
    const CRAFT = 1;
}

class ResRecipe
{
    const OK = 0;
    const NOT_LEARNT = 1;
    const LACKS_MATERIAL = 2;
    const INVALID_PARAM = 3;
    const LACKS_GOLD = 4;
    const LEVEL_NOT_MET = 5;
}

class eRecipeType
{
    const NONE = 0;
    const COOK = 1;
    const WORKBENCH = 2;
}

class ResCook extends ResRecipe
{
}

class ResCraft extends ResRecipe
{    
}

function parse_recipe_type(string $tp) : int {
    switch ($tp) {
        case 'FOOD':
            return eRecipeType::COOK;
        case 'T_MATERIAL':
        case 'TOY':
            return eRecipeType::WORKBENCH;
        default:
            break;
    }
    return eRecipeType::NONE;
}

function get_recipe_info(int $rid) {
    $db = DataRedis::get_db('recipe');
    if (!$db) {
        Response::error(ResCode::SQL_ERROR, '$db = DataRedis::get_db(recipe);');
    }

    $row = $db->get($rid);
    if (!$row) {
        return false;
    }

    $row['value'] = json_decode($row['value'], true);
    if (null === $row['value']) {
        return false;
    }
    $row['recipe_type'] = parse_recipe_type($row['recipe_type']);

    return $row;
}

function check_recipe_material(int $rid, int $type, int $rep) {
    $info = get_recipe_info($rid);
    if (!$info || !is_array($info)) {
        return ResRecipe::NOT_LEARNT;
    } else if (count($info) <= $type) {
        Response::error(ResCode::PARAM_ERR);
    }
    
    $gold = intval($info['need_gold']);
    if ($gold && $gold * $rep > User::GetGold()) {
        return ResRecipe::LACKS_GOLD;
    }

    $inven = User::GetInventory();
    foreach($info['value'][$type][0] as $item) {
        if ($inven->get_item_amount($item[0]) < $item[1] * $rep) {
            return ResRecipe::LACKS_MATERIAL;
        }
    }

    return ResRecipe::OK;
}

function do_craft(int $rid, int $type, int $rep, int $usage = Usage::COOK) {
    $info = get_recipe_info($rid);
    if (!$info) {
        Response::error(ResCode::SERVER_ERROR, '', new Exception('no recipe #' . $rid));
    }

    $rtype = $info['recipe_type'];
    if ((Usage::COOK === $usage && eRecipeType::COOK !== $rtype) ||
        (Usage::CRAFT === $usage && eRecipeType::WORKBENCH !== $rtype)) {
        $ret = ['rs' => ResRecipe::INVALID_PARAM];
        Response::addApiResponse('', 0, $ret);
        return $ret;
    }

    if (!check_craft_level($info)) {
        $ret = ['rs' => ResRecipe::LEVEL_NOT_MET];
        Response::addApiResponse('', 0, $ret);
        return $ret;
    }

    $mats = $info['value'][$type][0];
    $out = $info['value'][$type][1];
    $gold = intval($info['need_gold']);

    // build api response packet
    $rew = ['item' => [
        ['id' => intval($out[0]), 'amount' => intval($out[1]) * $rep]
    ]];

    // consume gold
    if ($gold) {
        User::UpdateGold(-1 * $gold * $rep, $usage);
    }

    // consume materials
    $inven = User::GetInventory();
    foreach($mats as $item) {
        $id = $item[0];
        $cnt = $item[1];
        if (0 >= $cnt) {
            continue;
        }
        $inven->update_item_amount($id, -1 * $cnt * $rep, $usage);
    }

    // rewards
    $rew = User::receive_rewards($rew, $usage);

    $ret = ['rs' => ResRecipe::OK, 'rid' => $rid, 'type' => $type, 'rep' => $rep, 'rew' => $rew];
    Response::addApiResponse('', 0, $ret);

    // Mission check
    if (Usage::COOK === $usage) {
        Mission::fire(eMissionTriggerType::COOK, $rep);
    } else if (Usage::CRAFT === $usage) {
        Mission::fire(eMissionTriggerType::CRAFT, $rep);
    }

    // level check
    User::check_level_event('recipe', $rid, $rep);

    // log
    H3Logger::log_recipe($rid, $type);

    return $ret;
}

function check_craft_level(array $info) {
    $req = $info['need_level'];
    if (0 == $req) {
        return true;
    }

    $cur = 0;
    if (eRecipeType::COOK === $info['recipe_type']) {
        $cur = User::get_countertop_level();
    } else if (eRecipeType::WORKBENCH === $info['recipe_type']) {
        $cur = User::get_workbench_level();
    }

    return $cur >= $req;
}

function object_for_toy_item(int $item_id) : int {
    try {
        $row = DataRedis::get_row('objects', strval($item_id));
    } catch (Exception $e) {
        Response::error(ResCode::REDIS_ERROR, '', $e);
    }

    if ($row) {
        return intval($row['object_id']);
    } else {
        return 0;
    }
}

// EOF
