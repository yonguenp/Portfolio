<?php

/**
 * 기획데이터 질의 처리
 * session token이 불필요함
 */

req_once(SRC_PATH . 'data/data.php');

switch(Response::getOpcode()) {
    // test case
    case 99:
    {
        if (!DEV) {
            Response::error(ResCode::PARAM_ERR);
        }

        $last_update = get_numeric_param('last_update');
        $tables = get_tables_list($last_update);

        $ret_data = [];
        foreach($tables as $tb_name) {
            $table_data = get_table_data_with_column($tb_name, $last_update);
            if($table_data) {
                $ret_data[] = $table_data;
            }
        }

        $data = [];
        $data['tb'] = $tables;
        $data['body'] = $ret_data;
        echo json_encode($data);
    } break;

    case OpData::REQ_DATA:
    {
        $last_update = get_numeric_param('last_update');
        $tables = get_tables_list($last_update);

        $ret_data = [];
        foreach($tables as $tb_name) {
            $table_data = get_table_data_with_column($tb_name, $last_update);
            if($table_data) {
                $ret_data[] = $table_data;
            }
        }

        if ($ret_data) {
            Response::addApiResponse(
                'data',
                OpData::REQ_DATA,
                ['rs' => ResData::OK, 'data' => $ret_data]
            );

        } else {
            Response::addApiResponse(
                'data',
                OpData::REQ_DATA,
                ['rs'=>ResData::UP_TO_DATE]
            );
        }

        Response::ok();
    } break;

    default:
        Response::error(ResCode::PARAM_ERR);
    break;
}

// EOF
