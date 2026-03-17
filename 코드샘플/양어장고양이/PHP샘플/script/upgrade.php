<?php

class OpUpgrade
{
    const UPGRADE = 1;
}

class ResUpgrade
{
    const OK = 0;
    const INVALID_PARAM = 1;
    const FULLY_UPGRADED = 2;
    const NOT_ENOUGH_GOLD = 3;
    const NOT_ENOUGH_ITEM = 4;
}

class eUpgradeTarget
{
    const NONE = 0;
    const FISHING_POLE = eObjectType::FISHING_POLE;
    const LITTERBOX = eObjectType::LITTERBOX;
    const AUTOMATIC_FEEDER = eObjectType::AUTOMATIC_FEEDER;
    const WHITE_CATHOUSE = eObjectType::WHITE_CATHOUSE;
    const WATER_FEEDER = eObjectType::WATER_FEEDER;
    const SEMIAUTO_LITTERBOX = eObjectType::SEMIAUTO_LITTERBOX;
    const INSULATED_CATHOUSE = eObjectType::INSULATED_CATHOUSE;
    const MOO_SCRATCHER = eObjectType::MOO_SCRATCHER;
    const CUSHION_BED = eObjectType::CUSHION_BED;
    const WOODEN_CATTOWER = eObjectType::WOODEN_CATTOWER;
    const SEMIAUTO_LITTERBOX2 = eObjectType::SEMIAUTO_LITTERBOX2;
    const CAMPING_BOX = eObjectType::CAMPING_BOX;
    const PIPE_CATTOWER = eObjectType::PIPE_CATTOWER;
    const SAMSAEK_BED = eObjectType::SAMSAEK_BED;
    const TREE_CATTOWER = eObjectType::TREE_CATTOWER;
    const CAT_WHEEL = eObjectType::CAT_WHEEL;
    const XMAS_CATTOWER = eObjectType::XMAS_CATTOWER;
    const SAMSAEK_HOUSE = eObjectType::SAMSAEK_HOUSE;
    const MOBILE_CATHOUSE = eObjectType::MOBILE_CATHOUSE;

    const SPOT_OBJECT_MAX = 100;
    const FISH_FARM = ePlantType::FISH_FARM;
    const FISH_TRAP = ePlantType::FISH_TRAP;
    const GIFT_BASKET = ePlantType::GIFT_BASKET;
    const WORKBENCH = 104;
    const COUNTERTOP = 105;
}

function res_upgrade_err(int $code) : void {
    Response::mainApiResponse(['rs' => $code]);
    Response::ok();
    exit;
}

function do_upgrade(int $what) : array {
    $rs = ResUpgrade::OK;

    // current level
    $level = get_upgrade_target_level($what);
    if (!$level || get_upgrade_level_max($what) <= $level) {
        res_upgrade_err(ResUpgrade::INVALID_PARAM);
    }

    // check cost
    $cost = get_upgrade_cost_info($what, $level);
    if (User::GetGold() < $cost['gold']) {
        res_upgrade_err(ResUpgrade::NOT_ENOUGH_GOLD);
    }
    $inven = User::GetInventory();
    foreach ($cost['item'] as $item) {
        if ($item[0] && $inven->get_item_amount($item[0]) < $item[1]) {
            error_log('not enough item for upgrade : ' . $item[0]);
            res_upgrade_err(ResUpgrade::NOT_ENOUGH_ITEM);
        }
    }

    // pay cost
    User::UpdateGold(-1 * $cost['gold'], Usage::UPGRADE);

    foreach ($cost['item'] as $item) {
        if ($item[0]) {
            $inven->update_item_amount($item[0], -1 * $item[1], Usage::UPGRADE);
        }
    }

    // alter
    switch ($what) {
        case eUpgradeTarget::FISH_FARM:
            upgrade_plant(ePlantType::FISH_FARM);
            User::SaveBaseInfo();
            break;
        case eUpgradeTarget::FISH_TRAP:
            upgrade_plant(ePlantType::FISH_TRAP);
            User::SaveBaseInfo();
            break;
        case eUpgradeTarget::GIFT_BASKET:
            upgrade_plant(ePlantType::GIFT_BASKET);
            User::SaveBaseInfo();
            break;
        case eUpgradeTarget::WORKBENCH:
            User::set_workbench_level($level + 1);
            User::SaveBaseInfo();
            break;
        case eUpgradeTarget::COUNTERTOP:
            User::set_countertop_level($level + 1);
            User::SaveBaseInfo();
            break;

        default:
            $obj = SpotObject::get($what);            
            if ($obj) {
                $obj->level_up();
                User::SaveBaseInfo();
            } else {
                $rs = ResUpgrade::INVALID_PARAM;
            }
            break;
    }

    Neco::on_upgrade($what, $level + 1);
    post_upgrade_event($what, $level + 1);

    return ['rs' => $rs, 'what' => $what, 'lvl' => $level + 1];
}

function get_upgrade_target_level(int $what) : int {
    switch ($what) {
        case eUpgradeTarget::FISH_FARM:
        case eUpgradeTarget::FISH_TRAP:
        case eUpgradeTarget::GIFT_BASKET:
            $plant = Plant::get($what);
            if ($plant) {
                return $plant->get_level();
            } else {
                return 0;
            }
            break;

        case eUpgradeTarget::WORKBENCH:
            return User::get_workbench_level();

        case eUpgradeTarget::COUNTERTOP:
            return User::get_countertop_level();
                    
        default:        
            break;                                                
    }

    if (eUpgradeTarget::SPOT_OBJECT_MAX > $what) {
        $obj = SpotObject::get($what);
        if ($obj) {
            return $obj->get_level();
        }
    }

    return 0;
}

function get_upgrade_cost_info(int $oid, int $level) : array {
    $db = DataRedis::get_db('upgrade_costs');
    if (!$db) {
        Response::error(ResCode::REDIS_ERROR, '!DataRedis::get_db(upgrade_costs);');
    }

    $k = strval($oid * 1000 + $level);
    $row = $db->get($k);
    if (!$row) {
        Response::error(ResCode::INVALID_DATA, 'no cost info for #' . $oid, ', level ' . $level);
    }

    $ret = [];
    $ret['gold'] = intval($row['need_gold']);
    $ret['item'] = [];
    for ($i = 1; 6 >= $i; ++$i) {
        $ino = intval($row['material' . $i]);
        $icnt = intval($row['material' . $i . '_count']);
        if ($ino && $icnt) {
            $ret['item'][] = [$ino, $icnt];
        }
    }

    return $ret;
}

function upgrade_plant(int $pid) {
    $p = Plant::get($pid);
    if (!$p || !$p->level_up()) {
        res_upgrade_err(ResUpgrade::INVALID_PARAM);
    }

    $p->save_sql();
    $p->write_state_packet();
}

function get_upgrade_level_max(int $what) : int {
    if (eUpgradeTarget::SPOT_OBJECT_MAX > $what) {
        return ObjectData::get_upgrade_level_max();;
    }

    switch ($what) {
        case eUpgradeTarget::FISH_FARM:
        case eUpgradeTarget::FISH_TRAP:
        case eUpgradeTarget::GIFT_BASKET:
            return PLANT_LEVEL_MAX;
        case eUpgradeTarget::WORKBENCH:
        case eUpgradeTarget::COUNTERTOP:
            return WORKBENCH_LEVEL_MAX;            
        default:
            break;
    }

    return 0;
}

function post_upgrade_event(int $what, int $level) : void {
    if (eUpgradeTarget::WORKBENCH === $what && 4 === $level) {
        Purchase::register_limited_avalable(
            eLimitedIap::WORKBENCH_LV4,
            Response::getTs() + 3 * SEC_HOUR
        );
    }

    User::check_level_event('upgrade', $what, $level);
}

// EOF
