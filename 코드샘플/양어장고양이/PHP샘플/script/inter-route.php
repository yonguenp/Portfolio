<?php

req_once(SRC_PATH . 'inter/inter.php');

validate_session_token();

switch(Response::getOpCode()) {
    case OpInter::TOUCH:
    {
        Response::mainApiResponse(['rs' => 0]);
        Response::ok();
        exit;

        $id = get_numeric_param('id');
        if (0 == $id) {
            $id = get_numeric_param('id');
        }
        if (!$id) {
            Response::error(ResCode::PARAM_ERR);
        }

        GameDb::startTransaction();

        $ret = User::DoTouch($id);
        if (ResInter::OK !== $ret['rs']) {
            Response::addApiResponse('', 0, $ret);
            Response::ok();
        }

        // reward
        $rew = Data::getTouchReward($id);        
        $rew = User::receive_rewards($rew, Usage::INTER_TOUCH);

        $ret['rew'] = $rew;
        Response::addApiResponse('', 0, $ret);

        User::SaveBaseInfo();
        GameDb::commit();
        Response::ok();
    } break; // case OpInter::TOUCH:

    case OpInter::PLAY:
    {
        Response::mainApiResponse(['rs' => 0]);
        Response::ok();
        exit;

        $id = get_numeric_param('id');
        if (0 == $id) {
            $id = get_numeric_param('id');
        }
        if (!$id) {
            Response::error(ResCode::PARAM_ERR);
        }

        GameDb::startTransaction();

        $ret = User::DoPlay($id);
        if (ResInter::OK !== $ret['rs']) {
            Response::addApiResponse('', 0, $ret);
            Response::ok();
        }

        // reward
        $rew = Data::getPlayReward($id);
        $rew = Response::addApiResponse('', 0, $ret);
        
        $ret['rew'] = $rew;
        User::receive_rewards($rew, Usage::INTER_PLAY);

        User::SaveBaseInfo();
        GameDb::commit();
        Response::ok();

    } break; // case OpInter::TOUCH:

    case OpInter::FEED:
    {

    } break; // case OpInter::TOUCH:

    case OpInter::IDLE:
    {
        Response::mainApiResponse(['rs' => 0]);
        Response::ok();
        exit;

        GameDb::startTransaction();

        $amount = User::RecieveIdleReward();
        $rew = ['gold' => $amount, 'exp' => 40];

        $ret = [];
        // backward compat
        $ret['dur'] = 0;
        $ret['last'] = Response::GetTs();
        
        if (0 < $amount) {
            $ret['rs'] = ResInter::OK;
            
            $rew = User::receive_rewards($rew, Usage::IDLE_REWARD);
            
            $ret['rew'] = $rew;
            Response::addApiResponse('', 0, $ret);

            Response::addApiResponse(
                'inter',
                OpInter::IDLE_STATE,
                User::GetIdle()->get_packet()
            );

            User::SaveBaseInfo();
            GameDb::commit();
        } else {
            $ret['rs'] = ResInter::IDLE_TOO_SHORT;
            Response::addApiResponse('', 0, $ret);

            GameDb::rollback();
        }
        
        Response::ok();
    } break; // case OpInter::IDLE:

    default:
        Response::error(ResCode::PARAM_ERR);
    break;
}

// EOF
