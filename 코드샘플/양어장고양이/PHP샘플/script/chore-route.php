<?php

req_once(SRC_PATH . 'chore/chore.php');

validate_session_token();

switch(Response::getOpCode()) {

case OpChore::DO_WORK:
{
    $res = ['rs' => ResChore::OK];    
    $rew = [];

    $gold = get_numeric_param('gold');
    if ($gold) {
        $rew['gold'] = $gold;
    }

    $catnip = get_numeric_param('catnip');
    if ($catnip) {
        $rew['catnip'] = $catnip;
    }

    $exp = get_numeric_param('exp');
    if ($exp) {
        $rew['exp'] = $exp;
    }

    $item = get_numeric_param('item');
    //if ($item && ItemType::NONE !== Data::get_item_info($item)['type']) {
    if ($item) {
        $amount = get_numeric_param('cnt', 1);
        $rew['item'] = [['id' => $item, 'amount' => $amount]];
    }

    if (empty($rew)) {
        $rew = get_chore_reward();
    }

    if (is_chore_moderate($rew)) {
        $res['rew'] = User::receive_rewards($rew, Usage::CHORE);
    } else {
        $res['rew'] = $rew;
    }

    User::SaveBaseInfoRedis();

    // $res = do_work();
    // if (ResChore::OK === $res['rs'] && isset($res['rew'])) {
    //     $res['rew'] = User::receive_rewards($res['rew'], Usage::CHORE);
    // }

    Response::mainApiResponse($res);
    Response::ok();
} break; // case OpChore::DO_WORK:

default:
    break;

} // switch(Resposne::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for chore');

// EOF
