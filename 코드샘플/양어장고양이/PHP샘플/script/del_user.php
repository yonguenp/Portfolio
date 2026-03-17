<?php
if (!isset($argc) || 2 > $argc) {
    exit;
}

$uno = intval($argv[1]);

require_once dirname(__FILE__) . '/../lib/common.php';

$TAR_SCHEMA = 'h3_game';
$TAR_COL = 'user_id';

try {
    $ret = GameDb::queryFirstColumn("SELECT `TABLE_NAME` FROM `information_schema`.`COLUMNS` WHERE `TABLE_SCHEMA`='{$TAR_SCHEMA}' AND `COLUMN_NAME`='{$TAR_COL}'");

    if (!$ret || empty($ret)) {
        echo 'no table to process' . PHP_EOL;
        exit;
    }

    echo 'Targets: ' . str_replace('"', '', json_encode($ret)) . PHP_EOL . PHP_EOL;

    GameDb::startTransaction();

    foreach($ret as $tb) {
        GameDb::query("DELETE FROM `{$tb}` WHERE `{$TAR_COL}`={$uno}");
        $ar = GameDb::affectedRows();
        if ($ar) {
            echo "{$ar} rows affected from {$tb}" . PHP_EOL;
        }
    }

    GameDb::commit();
    echo 'donezo.' . PHP_EOL;
} catch (Exception $e) {
    echo 'exception: ' . $e->getMessage() . PHP_EOL;
}
