<?php

req_once(SRC_PATH . 'upgrade.php');

validate_session_token();

switch (Response::getOpcode()) {

case OpUpgrade::UPGRADE:
{
    $what = get_numeric_param('what');
    if (!$what) {
        res_upgrade_err(ResUpgrade::INVALID_PARAM);
    }

    $trs = GameDb::getScopedTransaction();

    $res = do_upgrade($what);

    if (ResUpgrade::OK === $res['rs']) {
        $trs->commit();
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpUpgrade::UPGRADE:

default:
    break;
} // switch (Response::getOpcode()) {

Response::error(
    ResCode::PARAM_ERR,
    'invalid opcode for /upgrade. ' . Response::getOpcode()
);

// EOF
