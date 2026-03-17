<?php

req_once(SRC_PATH . '/cat/cat.php');
validate_session_token();

switch (Response::getOpCode()) {
    case OpCat::START_ACTION:
    {
        $catid = get_numeric_param('cat');
        $actid = get_numeric_param('act');

        $cat = Cat::get_cat(User::GetNo(), $catid);
        if (!$cat) {
            response_cat_error(eResCat::INVALID_CATID);
        }

        $rs = $cat->is_action_available($actid);
        if (eResCat::OK !== $rs) {
            response_cat_error($rs);
        }

        $act = CatData::get_action_data($actid);
        if (!$act) {
            response_cat_error(eResCat::INVALID_ACTIONID);
        }

        if (0 === $cat->get_fullness()) {
            response_cat_error(eResCat::NOT_ENOUGH_FULLNESS);
        }

        start_cat_action($catid, $actid);
        $res = [
            'rs' => eResCat::OK,
            'cat' => $catid,
            'act' => $actid
        ];

        Response::mainApiResponse($res);
        Response::ok();
    } break;// case OpCat::START_ACTION:

    case OpCat::FINISH_ACTION:
    {
        $actid = get_ongoing_action();
        if (!$actid) {
            response_cat_error(eResCat::NOT_ON_ACTION);
        }

        $data = CatData::get_action_data($actid);
        if (!$data) {
            Response::error(ResCode::SERVER_ERROR, 'action ' . $actid . ' not exists');
        }

        $catid = $data['cat'];
        $cat = Cat::get_cat(User::GetNo(), $catid);
        if (!$cat) {
            Response::error(ResCode::SERVER_ERROR, 'user does not has cat ' . $catid);
        }

        $trs = GameDb::getScopedTransaction();

        $res = finish_cat_action($cat, $data);
        if (eResCat::OK !== $res['rs']) {
            Response::mainApiResponse($res);
            Response::ok();
        }

        if (isset($res['rew']) && !empty($res['rew'])) {
            $res['rew'] = User::receive_rewards($res['rew'], Usage::CAT_ACTION_REWARD);
        }

        $trs->commit();
        Response::mainApiResponse($res);
        Response::ok();
    } break;// case OpCat::FINISH_ACTION:
    
    case OpCat::OPEN_ACTION:
    {
        $catid = get_numeric_param('cat');
        if (!$catid || !($cat = Cat::get_cat(User::GetNo(), $catid))) {
            response_cat_error(eResCat::INVALID_CATID);
        }
        
        $data = CatData::get_cat_data($catid);
        if (!$data) {
            Response::error(ResCode::SERVER_ERROR, 'no cat data for cat #' . $catid);
        }

        if (count($data['action']) <= $cat->get_action_step()) {
            response_cat_error(eResCat::NO_MORE_ACTION);
        }

        $actid = $data['action'][$cat->get_action_step()];
        $actdata = CatData::get_action_data($actid);
        if (!$actdata) {
            Response::error(ResCode::SERVER_ERROR, 'no action data for act #' . $actid);
        }

        $trs = GameDb::getScopedTransaction();

        $res = open_cat_action($cat, $actdata);
        if (eResCat::OK === $res['rs']) {
            User::SaveBaseInfo();
            $trs->commit();
        }

        Response::mainApiResponse($res);
        Response::ok();
    } break;// case OpCat::OPEN_ACTION:

    case OpCat::LEVEL_UP:
    {
        $catid = get_numeric_param('cat');
        if (!$catid || !($cat = Cat::get_cat(User::GetNo(), $catid))) {
            response_cat_error(eResCat::INVALID_CATID);
        }

        $data = CatData::get_cat_data($catid);
        if (!$data) {
            Response::error(ResCode::SERVER_ERROR, 'no cat data for cat #' . $catid);
        }

        $trs = GameDb::getScopedTransaction();

        $res = increase_cat_level($cat, $data);
        if (eResCat::OK === $res['rs']) {
            if (isset($res['rew'])) {
                $res['rew'] = User::receive_rewards($res['rew'], Usage::CAT_LEVEL_REWARD);
            } else {
                User::SaveBaseInfo();
            }
            $trs->commit();
        }

        Response::mainApiResponse($res);
        Response::ok();
    } break; // case OpCat::LEVEL_UP:

    case OpCat::FEED:
    {
        $catid = get_numeric_param('cat');
        $item = get_numeric_param('food');
        $cnt = get_numeric_param('cnt', 1);

        $cat = Cat::get_cat(User::GetNo(), $catid);
        if (!$cat) {
            response_cat_error(eResCat::INVALID_CATID);
        }

        $trs = GameDb::getScopedTransaction();

        $res = feed_cat($cat, $item, $cnt);
        if (eResCat::OK === $res['rs']) {
            $trs->commit();
        }

        Response::mainApiResponse($res);
        Response::ok();
    } break; // case OpCat::FEED:

    case OpCat::TAKE_PICTURE:
    {
        $catid = get_numeric_param('cat');

        $cat = Cat::get_cat(User::GetNo(), $catid);
        if (!$cat) {
            response_cat_error(eResCat::INVALID_CATID);
        }

        $trs = GameDb::getScopedTransaction();
        
        $res = take_cat_picture($cat);
        if (eResCat::OK === $res['rs']) {
            $trs->commit();
        }

        Response::mainApiResponse($res);
        Response::ok();
    } break;// case OpCat::TAKE_PICTURE:

    default:
        break;
                                                                
} // switch (Response::getOpCode()) {

Response::error(ResCode::PARAM_ERR, 'uri: cat, op: ' . Response::getOpCode() . ' was not switched');

// EOF
