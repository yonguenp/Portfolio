<?php

require_once SRC_PATH . '/attendance.php';

validate_session_token();

switch (Response::getOpcode()) {

case OpAttendance::STATE:
{
    $at = Attendance::get_instance();
    $pk = $at->to_packet();
    $pk['rs'] = eResAttendance::OK;

    // over extended attendance
    if (eResAttendance::COMPLETED === $at->state_as_enum()) {        
        GameDb::startTransaction();
        if (true === $at->set_attendance()) {
            $at->save_sql();
            GameDb::commit();
        } else {
            GameDb::rollback();
        }
    }

    Response::mainApiResponse($pk);
    Response::ok();

} break; // case OpAttendance::STATE:

case OpAttendance::REWARD:
{
    $at = Attendance::get_instance();

    GameDb::startTransaction();

    $res = $at->take_reward();
    if (eResAttendance::OK === $res['rs']) {
        $res['rew'] = User::receive_rewards($res['rew'], Usage::ATTENDANCE);

        $at->save_sql();
        GameDb::commit();
    } else {
        GameDb::rollback();
    }

    Response::mainApiResponse($res);
    Response::ok();

} break; // case OpAttendance::REWARD:

} // switch (Response::getOpcode()) {

Response::error(ResCode::PARAM_ERR, 'invalid op for attendance: ' . Response::getOpcode());

// EOF
