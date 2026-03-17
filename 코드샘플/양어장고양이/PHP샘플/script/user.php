<?php

function get_tutorial_rewards_table() : ?array {
    try {
        $db = DataRedis::get_db('neco_tutorial_rewards');
        if (!$db) {
            Response::error(ResCode::REDIS_ERROR, 'no dataredis neco_tutorial_rewards');
        }
        $raw = $db->get_all();
    } catch (Exception $e) {
        Response::error(ResCode::REDIS_ERROR, '', $e);
    }

    $ret = [];
    foreach ($raw as $id => $row) {
        $ret[$id] = [
            $row['reward_type'],
            intval($row['reward_id']),
            intval($row['reward_count'])
        ];
    }

    ksort($ret, SORT_NUMERIC);

    return $ret;
}

function build_tutorial_reward_array(int $from, int $to) : ?array {
    if ($from >= $to) {
        return null;
    }

    $table = get_tutorial_rewards_table();
    if (!$table || !is_array($table)) {
        return null;
    }

    $rew = [];
    foreach ($table as $lvl => $data) {
        if ($lvl <= $from) {
            continue;
        } else if ($lvl > $to) {
            break;
        }

        $good = new Good($data[0], $data[1], $data[2]);
        if ($good) {
            $good->append_reward_array($rew);
        }
    }

    return $rew;
}

function check_tutorial_finish(int $before, int $after) {
    if (PROLOGUE_STAGE_FINISH <= $before ||
        PROLOGUE_STAGE_FINISH > $after) {
            return;
    }

    User::SetBaseInfo(UserInfoKey::TUTORIAL_FINISH, Response::getTs());
    
    Purchase::register_limited_avalable(eLimitedIap::TUTORIAL, Response::getTs() + SEC_DAY);
    
    send_tutorial_greeting_mail();

    feed_for_samsaek();

    User::check_level_event('tutorial', 0);
}

function on_offerwall_callback() : void {
    $api_id = get_param('id');
    $curr = get_numeric_param('currency');
    $user_id = get_numeric_param('snuid');
    $hash = get_param('verifier');

    if (!$curr || !$user_id) {
        http_response_code(403);
        return;
    }

    $sks = ['NEFEhd9dDboRYP42wfgQ', 'NAa2BpNIl36lVGwJyTvX'];

    $success = false;
    foreach ($sks as $sk) {
        $test = implode(':', [$api_id, $user_id, $curr, $sk]);
        if ($hash === md5($test)) {
            $success = true;
            break;
        }
    }

    if (!$success) {
        http_response_code(403);
        return;
    }

    req_once(SRC_PATH . '/post.php');

    $row = [
        'title' => '포인트 상점',
        'body' => '',
        'attach_type' => ePostAttachType::CURRENCY,
        'attach_id' => eCurrency::POINT,
        'attach_count' => $curr
    ];

    if (1 === send_post_to_user($user_id, $row)) {
        http_response_code(200);
    } else {
        http_response_code(403);
    }
    
    exit;
}

function send_post_to_user(int $user_id, array $rows) : int {
    if (empty($rows)) {
        return 0;
    }

    if (isset($rows[0]) && is_array($rows[0])) {
        $len = count($rows);
        for ($i = 0; $len > $i; ++$i) {
            $rows[$i]['user_id'] = $user_id;
            $rows[$i]['state'] = ePostState::NEW;
            $rows[$i]['register_time'] = Response::getTs();
            $rows[$i]['expire_time'] = 0;
        }
    } else {
        $rows['user_id'] = $user_id;
        $rows['state'] = ePostState::NEW;
        $rows['register_time'] = Response::getTs();
        $rows['expire_time'] = 0;
    }

    try {
        GameDb::insert('posts', $rows);
        PostItem::write_last_post_packet(true);
        return GameDb::affectedRows();
    } catch (Exception $e) {
        Response::error(ResCode::SQL_ERROR, '', $e);
    }


    return 0;
}

function send_tutorial_greeting_mail() : void {
    req_once(SRC_PATH . '/post.php');
    $row = [
        'title' => SBLocale::get_text('TUTO_POST_T'),
        'body' => SBLocale::get_text('TUTO_POST_B'),
        'attach_type' => ePostAttachType::ITEM,
        'attach_id' => PROLOGUE_FINISH_REWARD_ID,
        'attach_count' => PROLOGUE_FINISH_REWARD_CNT
    ];

    $user_id = User::GetNo();
    assert(0 !== $user_id);
    send_post_to_user($user_id, $row);
}

function feed_for_samsaek() : void {
    $SAMSAEK_TUTORIAL_FEED = 92;
    $SAMSAEK_TUTORIAL_FEED_BOWL = 102;

    VisitManager::feed($SAMSAEK_TUTORIAL_FEED, $SAMSAEK_TUTORIAL_FEED_BOWL);
    VisitManager::save_dirty();
    VisitManager::write_state_packet();
}

// EOF
