<?php

class eResPlant
{
    const OK = 0;
    const NOTHING_TO_HARVEST = 1;
    const INVALID_ID = 2;
    const SERVER_ERROR = 3;
    const NOT_ENOUGH_COST = 4;
    const ALREADY_BOOSTED = 5;
}

function res_plant_error(int $err) {
    Response::mainApiResponse(['rs' => $err]);
    Response::ok();
    exit;
}

function try_harvest(int $id) {
    $plant = Plant::get($id);
    if (!$plant) {
        return ['rs' => eResPlant::INVALID_ID];
    }

    $plant->advance(Response::getTs());
    $rew = $plant->harvest(Response::getTs());
    if (!$rew || empty($rew)) {
        return ['rs' => eResPlant::NOTHING_TO_HARVEST];
    }
    $plant->save_sql();
    $plant->write_state_packet();

    $ret = [];
    $ret['rs'] = eResPlant::OK;
    $ret['id'] = $id;
    $ret['rew'] = $rew;

    return $ret;
}

function apply_plant_boost(int $id, bool $ad) : array {
    $res = ['id' => $id];

    // cost check
    // plant check
    if ($ad) {
        $info = PlantData::get_ad_booster_info();
        $type = ePlantBoosterType::AD;
    } else {
        $info = PlantData::get_booster_info($id);
        $type = ePlantBoosterType::PAID;
    }
    $plant = Plant::get($id);
    if (!$info || !$plant) {
        $res['rs'] = eResPlant::INVALID_ID;
        return $res;
    }

    if ($plant->is_boosted(Response::getTs())) {
    //     $res['rs'] = eResPlant::ALREADY_BOOSTED;
    //     return $res;
    }

    if ($info['price'] > User::get_catnip()) {
        $res['rs'] = eResPlant::NOT_ENOUGH_COST;
        return $res;
    }

    GameDb::startTransaction();

    // apply
    if (!$plant->apply_boost($type)) {
        GameDb::rollback();
        $res['rs'] = eResPlant::SERVER_ERROR;
        return $res;
    }
    $plant->save_sql();
    $plant->write_state_packet();

    // pay cost
    User::update_currency(eCurrency::CATNIP, -1 * $info['price'], Usage::PLANT_BOOSTER);
    User::SaveBaseInfo();

    GameDb::commit();

    $res['rs'] = eResPlant::OK;
    $res['exp'] = $plant->get_booster_exp();

    return $res;
}

// EOF
