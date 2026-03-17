<?php

define('DEV', TRUE);
define('STAGE', 'dev');
//define('DEV', FALSE);
//define('STAGE', 'release');

header('Content-Type: application/json');
require_once dirname(__FILE__) . '/../lib/common.php';

define('LOG_DBNAME', 'h3_log');
define('P_PREF', 'p_');
define('STR_RESERVED_MONTH', '+3 month');

/**
 * main
 */

// void main(int) {

// 컨트롤 테이블에 리스팅된 테이블별 로그 보관 주기와 현재 파티션 상태 정보 획득
$part = [];
$peri = [];
load_partition_info($part, $peri);

// 각 테이블별로 다음달 파티션 생성, 보관주기 지난 파티션 드랍
$logs = [];
$temps = [];
foreach($part as $k => $v) {
    $logs[$k] = handle_table($k, $v, $peri[$k], $temps);
}

drop_temp_tables($temps);

$logs['elap'] = microtime(true) - $_SERVER['REQUEST_TIME_FLOAT'];
echo json_encode($logs) . PHP_EOL;
// }

/**
 * __log_retention table에 명시된 모든 테이블의 파티션 정보 읽어 정리하여 리턴
 *
 * @return void
 */
function load_partition_info(array &$partition, array &$period) {
    $q = 'SELECT P.TABLE_NAME, P.PARTITION_NAME, LR.retention_period FROM __log_retention LR INNER JOIN information_schema.PARTITIONS P ON P.TABLE_SCHEMA=%s AND P.TABLE_NAME=LR.log_table_name';
    $res = LogDb::query($q, LOG_DBNAME);
    if (!$res) {
        return;
    }
    
    foreach($res as $row) {
        $tb = $row['TABLE_NAME'];
        
        if (!isset($partition[$tb])) {
            $partition[$tb] = [];
        }

        if ($row['PARTITION_NAME']) {
            $partition[$tb][] = $row['PARTITION_NAME'];
        }
        
        $period[$tb] = intval($row['retention_period']);
    }

    foreach($partition as $k => $v) {
        asort($partition[$k]);
    }
}

function handle_table(string $tb, array $parts, int $period, array &$temps) {
    $first_p = P_PREF . date('Y-m', strtotime("-{$period} month"));
    $last_m = date('Y-m', strtotime(STR_RESERVED_MONTH));

    $log = '';

    // cursor
    if (empty($parts)) {
        // create one
        $first_row = find_oldest_data($tb);
        if($first_row) {
            // 기존 데이터 있으면 해당 데이터가 포함될 파티션
            $cur_m = substr($first_row, 0, 7);
            $log .= 'initialize ' . $cur_m . ' from oldest data. ';
        } else {
            // 빈 테이블이면 이번 달
            $cur_m = date('Y-m');
            $log .= 'initialize ' . $cur_m . ' for empty table. ';
        }

        // 생성
        create_initial_partition($tb, $cur_m);
        $parts[] = P_PREF . $cur_m;
    } else {
        // 최신 파티션
        $cur_m = substr($parts[count($parts)-1], strlen(P_PREF));
    }

    // +2달까지 미리 생성
    while($cur_m < $last_m) {
        $cur_m = split_next_partition($tb, $cur_m, 1);
        $parts[] = P_PREF . $cur_m;
        $log .= 'create partition ' . P_PREF . $cur_m . '. ';
    }

    // 오래된 파티션 드랍
    $drops = [];
    for ($i = 0; count($parts) > $i; ++$i) {
        if ($parts[$i] >= $first_p) {
            break;
        }
        $drops[] = $parts[$i];
        $log .= 'dropping ' . $parts[$i] . '. ';
    }
    if (!empty($drops)) {
        drop_partitions($tb, $drops, $temps);
    }

    return $log;
}

function find_oldest_data(string $tb) {
    $q = "SELECT `log_time` FROM `{$tb}` LIMIT 1";
    $res = LogDb::queryFirstField($q, $tb);

    return $res;
}

function create_initial_partition(string $tb, string $str_t) {
    $str_p = P_PREF . $str_t;
    $q = "ALTER TABLE `{$tb}` PARTITION BY RANGE COLUMNS (`log_time`) (PARTITION `{$str_p}` VALUES LESS THAN MAXVALUE);";
    LogDb::query($q);
}

function split_next_partition(string $tb, string $cur_m, int $incr = 1) {
    $next_m = date('Y-m', strtotime($cur_m . " +{$incr} month"));

    $cur_p = P_PREF . $cur_m;
    $next_p = P_PREF . $next_m;

    $q = "ALTER TABLE `{$tb}` REORGANIZE PARTITION `{$cur_p}` INTO (PARTITION `{$cur_p}` VALUES LESS THAN (%s), PARTITION `{$next_p}` VALUES LESS THAN MAXVALUE);";
    LogDb::query($q, date('Y-m-d H:i:s', strtotime($next_m)));

    return $next_m;
}

function drop_partitions(string $tb, array $parts, array &$temps) {
    $tmp = $tb . '_tmp';
    // create tmp table
    //LogDb::query("CREATE TABLE IF NOT EXISTS {$tmp} SELECT * FROM {$tb} WHERE 0");
    LogDb::query("DROP TABLE IF EXISTS `{$tmp}`");
    LogDb::query("CREATE TABLE {$tmp} LIKE {$tb}");
    $temps[] = $tmp;

    // delete partitions
    try {
        LogDb::query("ALTER TABLE {$tmp} REMOVE PARTITIONING");
    } catch (Exception $e) {

    }

    foreach($parts as $p) {
        // migrate
        LogDb::query("ALTER TABLE `{$tb}` EXCHANGE PARTITION `{$p}` WITH TABLE `{$tmp}`");
        // delete
        LogDb::query("ALTER TABLE `{$tb}` DROP PARTITION `{$p}`");
    }

    // delete tmp table
    //LogDb::query("DROP TABLE `{$tmp}`");
}

function drop_temp_tables(array $temps) {
    if(empty($temps)) {
        return;
    }
    $str_tb = implode('`, `', $temps);
    LogDb::query("DROP TABLE IF EXISTS `{$str_tb}`");
}

// EOF
