<?php

/**
 * data.php
 * 기획데이터 질의 처리
 */

class OpData
{
    const REQ_DATA = 1;
}

class ResData
{
    const OK = 0;
    const UP_TO_DATE = 1;
}

function get_tables_list(int $last_update) {
    $q = 'SELECT `table_name` FROM `_control`'
       . ' WHERE `send_client`=1 AND `data_update`>=FROM_UNIXTIME(%i);';
    try {
        $res = DataDb::queryFirstColumn($q, $last_update);
        if (is_array($res)) {
            return $res;
        } else {
            Response::error(ResCode::SQL_ERROR);
        }
    } catch(Exception $e) {
        Response::error(ResCode::SQL_ERROR);
    }
}

function get_table_data_with_column(string $tb_name, int $last_update) {
    static $src = null;
    if (null == $src) {
        $src = include CONFIG_PATH . 'data-tables.php';
    }        

    if(!array_key_exists($tb_name, $src)) {
        return false;
    }

    $data = $src[$tb_name];

    $ret = [];
    $ret['head'] = [];
    $ret['head']['name'] = $tb_name;
    $ret['head']['sql'] = $data[0];
    $ret['head']['col'] = $data[1];
    $ret['head']['col_type'] = $data[2];
    
    $cols = implode('`, `', $data[1]);
    //$q = "SELECT `{$cols}` FROM `{$tb_name}` WHERE `last_update`>=FROM_UNIXTIME(%i)";
    $q = "SELECT `{$cols}` FROM `{$tb_name}`";

    try {
        $res = DataDb::query($q, $last_update);
        if (!$res) {
            return false;
        }
        $body = [];
        foreach($res as $row) {
            $tmp = [];
            foreach($data[1] as $col_name) {
                $tmp[] = $row[$col_name];
            }

            $body[] = $tmp;
        }
        $ret['body'] = $body;

        return $ret;
    } catch(Exception $e) {
        return false;
    }
}

// EOF
