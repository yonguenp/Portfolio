<?php

validate_session_token();
req_once(SRC_PATH . 'iap.php');

switch (Response::getOpcode()) {

case OpIap::CHECK:
{
    $prod = get_numeric_param('prod');

    $pc = Purchase::get_instance();

    $rs = $pc->check_availability($prod);
    Response::mainApiResponse(['rs' => $rs]);

    Response::ok();
} break; // case OpIap::CHECK:

case OpIap::POST_PAYMENT:
{
    $prod = get_numeric_param('prod');
    $str_receipt = get_param('receipt');
    $res = ['prod' => $prod];

    $pc = Purchase::get_instance();

    // DEV Log
    $pc->write_purchase_state_dev($prod, $str_receipt);

    // broken receipt string
    $param_body = json_decode($str_receipt, true);
    if (!$param_body || !isset($param_body['Payload'])) {
        error_log('[IAP] 1');
        on_purchase_fail(eResIap::INVALID_RECEIPT, $prod, $str_receipt);
    }
    
    // $pc = Purchase::get_instance();

    GameDb::startTransaction();

    // 구매 가능한 상태인지
    $rs = $pc->check_availability($prod);
    if (eResIap::OK !== $rs) {
        //error_log('[IAP] 2');
        on_purchase_fail($rs, $prod, $param_body);
    }

    // 옳바른 영수증인가
    $rs = $pc->validate_receipt($prod, $param_body);
    if (eResIap::OK !== $rs) {
        //error_log('[IAP] 3');
        on_purchase_fail($rs, $prod, $param_body);
    }

    // 영수증 정보 기입
    $rs = $pc->write_purchase_state($prod, $param_body);
    if (eResIap::OK !== $rs) {
        //error_log('[IAP] 4');
        on_purchase_fail($rs, $prod, $param_body);
    }
    
    // 보상 상품 지급
    $rs = handle_iap_reward($prod, $res);
    $res['rs'] = $rs;
    if (eResIap::OK !== $rs) {
    //if (!PackageData::send_mail($prod, 'In-App-Purchase', 'Contents', 0)) {
        on_purchase_fail($rs, $prod, $param_body);
    }

    if (isset($res['rew']) && is_array($res['rew'])) {
        $res['rew'] = User::receive_rewards($res['rew'], Usage::SHOP);
    }
    
    if (0 === User::GetBaseInfo(UserInfoKey::IAP_COUNT)) {
        send_iap_package_mail(IAP_FIRST_REWARD_PACKAGE);
        $res['gift'] = IAP_FIRST_REWARD_PACKAGE;
    }

    // 기간 한정인지
    if (1) {
        Purchase::disable_limited_avalable($prod);
    }

    // 결제 횟수
    User::SetBaseInfo(
        UserInfoKey::IAP_COUNT,
        User::GetBaseInfo(UserInfoKey::IAP_COUNT) + 1
    );
    User::SaveBaseInfo();
    $res['first'] = (0 === User::GetBaseInfo(UserInfoKey::IAP_COUNT));

    GameDb::commit();

    H3Logger::log_iap($prod, $pc->get_market(), $pc->get_sku(), eIapState::PURCHASED);

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpIap::POST_PAYMENT:

case OpIap::PURCHASE_COUNT:
{
    $cnt = Purchase::get_limited_purchase_count_user();
    Response::mainApiResponse(['rs' => eResIap::OK, 'cnt' => $cnt]);
    Response::ok();

} break; // case OpIap::PURCHASE_COUNT:

case OpIap::FULL_INFO:
{
    $res = Purchase::get_state_packet();
    $res['rs'] = eResIap::OK;
    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpIap::FULL_INFO:

case OpIap::LIMITED_INFO:
{
    $res = ['rs' => eResIap::OK];
    $limit = Purchase::get_limited_list();
    $res['limit'] = $limit ? $limit : new stdClass;
    $res['first'] = intval(0 === User::GetBaseInfo(UserInfoKey::IAP_COUNT));

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpIap::LIMITED_INFO:

default:
    break;

} // switch (Response::getOpcode()) {

Response::error(ResCode::PARAM_ERR, 'route-iap uncategorized op ' . Response::getOpCode());

// EOF
