<?php

validate_session_token();
req_once(SRC_PATH . '/shop.php');

switch (Response::getOpcode()) {

case OpShop::PURCHASE:
{
    $prod = (int)get_numeric_param('prod');
    $list = get_numeric_param('list');
    $cnt = (int)get_numeric_param('cnt', 1);

    $trs = GameDb::getScopedTransaction();

    if (eShopList::STATIC === $list) {
        $res = purchase_shop_item_static($prod, $cnt);
    } else {
        $res = purchase_shop_item_dynamic($prod, $cnt);
    }

    if (eResShop::OK === $res['rs']) {
        User::SaveBaseInfo();
        $trs->commit();
    }

    Response::mainApiResponse($res);
    Response::ok();

} // case OpShop::PURCHASE: {

case OpShop::GET_LIST:
{
    
    $res = [];
    $res['rs'] = eResShop::OK;

    $ads = AdLimiter::get();
    
    $fish = DynamicShop::get(eDynamicShopType::FISH);
    $res['fish'] = $fish->get_list();
    $res['fish_refresh'] = $fish->get_refresh_time();
    $res['fish_ad'] = $ads->get_count(AdLimiter::TYPE_FISH);
    
    $hw = DynamicShop::get(eDynamicShopType::HARDWARE);
    $res['hardware'] = $hw->get_list();
    $res['hardware_refresh'] = $hw->get_refresh_time();
    $res['hardware_ad'] = $ads->get_count(AdLimiter::TYPE_HARDWARE);

    Response::mainApiResponse($res);
    Response::ok();

} // case OpShop::GET_LIST:

case OpShop::REFRESH_LIST:
{
    $type = (int)get_numeric_param('type');
    if (eDynamicShopType::FISH !== $type &&
        eDynamicShopType::HARDWARE !== $type) {
        Response::error(ResCode::PARAM_ERR, 'Invalid dynamic shop type ' . $type);
    }
    $catnip = boolval(get_numeric_param('catnip'));
    $ad = boolval(get_numeric_param('ad'));

    $trs = GameDb::getScopedTransaction();

    $res = refresh_dynamic_shop_list($type, $catnip, $ad);
    if (eResShop::OK === $res['rs']) {
        $trs->commit();
    }

    Response::mainApiResponse($res);
    Response::ok();
}

default:
    break;

} // switch (Response::getOpcode()) {

Response::error(ResCode::PARAM_ERR, 'route-shop uncategorized op ' . Response::getOpcode());

// EOF
