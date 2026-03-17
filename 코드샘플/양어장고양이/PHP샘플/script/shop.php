<?php

class OpShop
{
    const NONE = 0;
    const PURCHASE = 1;
    const GET_LIST = 2;
    const REFRESH_LIST = 3;
}

class eResShop
{
    const OK = 0;
    const NO_SUCH_PRODUCT = 1;    // 존재 않는 상품 id를 전달받음
    const NOT_FOR_SALE = 2;       // 존재하나 현재 api로 구입할 수 없음(인앱 상품)
    const COST_NOT_ENOUGH = 3;    // 가난하여 결제 불가
    const OUT_OF_STOCK = 4;       // 변동목록 상품은 한정 수량임
    const NOT_AVAILABLE_NOW = 5;  // 변동목록 상품이 현재 비활성화 상태
    const NEED_TO_WAIT = 6;       // 목록 갱신하려면 더 기다려야 함
    const NO_CATNIP = 7;          // 캣닙 사용 시도하였으나 보유하지 않음
    const ADS_EXHAUSTED = 8;        // 광고시청 갱신 시도하였으나 일 5회 제한 소진
}

class eShopCategory
{
    const COMMON = 0;
    const PACKAGE = 1;
    const CATNIP = 2;
    const POINT = 3;
}

class eDynamicShopType
{
    const FISH = 1;
    const HARDWARE = 2;
}

function purchase_shop_item_static(int $prod, int $cnt) : array {        
    $ret = ['prod' => $prod, 'list' => eShopList::STATIC, 'cnt' => $cnt];

    $info = ShopData::get_static_good_info($prod);
    if (!$info) {
        $ret['rs'] = eResShop::NO_SUCH_PRODUCT;
        return $ret;
    }

    // check limited purchase
    $is_limited = (eIapLimitType::NONE !== $info->ltype);
    $limit_expire = 0;
    $num_purchases = 0;
    if ($is_limited) {
        req_once(SRC_PATH . 'iap.php');
        $limit_expire = get_iap_period_end($info->ltype);
        $num_purchases = load_limited_purchase_count($prod, $limit_expire);

        if ($info->lvalue < $num_purchases + $cnt) {
            $ret['rs'] = eResShop::OUT_OF_STOCK;
            return $ret;
        }
    }

    // check price
    if (!$info->is_price_ok($cnt)) {
        $ret['rs'] = eResShop::COST_NOT_ENOUGH;
        return $ret;
    }

    // pay price
    $info->pay_price($cnt);

    // get rewards
    $rew = $info->to_reward_array($cnt);
    if (!empty($rew)) {
        $ret['rew'] = User::receive_rewards($rew, Usage::SHOP);
    } else {
        if (eShopGoods::PACKAGE === $info->type) {
            req_once(SRC_PATH . 'post.php');            
            PackageData::send_mail($info->id, 0);
        }
        User::SaveBaseInfo();
    }

    if ($is_limited) {
        try {
            $ins = [
                'user_id' => User::GetNo(),
                'prod_id' => $prod,
                'num_purchases' => $num_purchases + $cnt,
                'period_expire' => $limit_expire
            ];
            $upd = [
                'num_purchases' => $num_purchases + $cnt,
                'period_expire' => $limit_expire
            ];

            GameDb::insertUpdate(
                'limited_purchases',
                $ins,
                $upd
            );
        } catch (Exception $e) {
            Response::error(ResCode::SQL_ERROR, '', $e);
        }
    }

    $ret['rs'] = eResShop::OK;
    return $ret;
}

function purchase_shop_item_dynamic(int $prod, int $cnt) : array {
    $ret = ['prod' => $prod, 'list' => eShopList::DYNAMIC, 'cnt' => $cnt];

    $info = ShopData::get_dynamic_good_info($prod);
    if (!$info) {
        $ret['rs'] = eResShop::NO_SUCH_PRODUCT;
        return $ret;
    }

    // check stock
    $list = DynamicShop::get($info->market_type);
    if (!$list) {
        Response::error(ResCode::SERVER_ERROR, 'could not load dynamic market list for ' . $info->market_type);
    }
    if ($cnt > $list->get_stock($prod)) {
        $ret['rs'] = eResShop::OUT_OF_STOCK;
        return $ret;
    }

    // check price
    if (!$info->is_price_ok($cnt)) {
        $ret['rs'] = eResShop::COST_NOT_ENOUGH;
        return $ret;
    }

    // pay price
    $info->pay_price($cnt);

    // get rewards
    $rew = $info->to_reward_array($cnt);
    if (!empty($rew)) {
        $ret['rew'] = User::receive_rewards($rew, Usage::SHOP);
    }

    // modify stock
    $list->decrease_stock($prod, $cnt);

    $ret['rs'] = eResShop::OK;
    return $ret;
}

function load_limited_purchase_count(int $prod, int $exp) : int {
    try {
        if ($exp) {
            $q = 'SELECT `num_purchases` FROM `limited_purchases` WHERE `user_id`=%i AND `prod_id`=%i AND `period_expire`>=%i';
            $res = GameDbRo::queryFirstField($q, User::GetNo(), $prod, $exp);
        } else {
            $q = 'SELECT `num_purchases` FROM `limited_purchases` WHERE `user_id`=%i AND `prod_id`=%i';
            $res = GameDbRo::queryFirstField($q, User::GetNo(), $prod);
        }
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }

    return intval($res);
}

function refresh_dynamic_shop_list(int $type, bool $catnip, bool $ad) : array {
    $ret = ['type' => $type];

    $list = DynamicShop::get($type);
    if (!$list) {
        Response::error(ResCode::SERVER_ERROR, 'No dynamic list returned for ' . $type);
    }

    $ret['rs'] = eResShop::OK;
    if ($list->get_refresh_time() > Response::getTs()) {
        if ($catnip && DYNAMIC_SHOP_REFRESH_COST > User::get_catnip()) {
            $ret['rs'] = eResShop::NO_CATNIP;
        } else if ($ad && !AdLimiter::get()->use_count($type)) {
            $ret['rs'] = eResShop::ADS_EXHAUSTED;
        } else if (!$catnip && !$ad) {
            $ret['rs'] = eResShop::NEED_TO_WAIT;
        }

        if (eResShop::OK !== $ret['rs']) {
            $k = (eDynamicShopType::FISH === $type) ? 'fish_refresh' : 'hardware_refresh';
            $ret[$k] = $list->get_refresh_time();
            return $ret;
        }
    } else {
        $catnip = false;
        $ad = false;
    }

    // use catnip
    if ($catnip && DYNAMIC_SHOP_REFRESH_COST <= User::get_catnip()) {
        User::update_catnip(-1 * DYNAMIC_SHOP_REFRESH_COST, Usage::SHOP);
        User::SaveBaseInfo();
        $ret['catnip'] = DYNAMIC_SHOP_REFRESH_COST;
    }

    $list->refresh_list();
    $list->save_sql();

    $pref = eDynamicShopType::FISH === $type ? 'fish' : 'hardware';
    $ret[$pref] = $list->get_list();
    $ret[$pref . '_refresh'] = $list->get_refresh_time();
    if ($ad) {
        $ret[$pref . '_ad'] = AdLimiter::get()->get_count($type);
    }

    $ret['rs'] = eResShop::OK;
    return $ret;
}

// EOF
