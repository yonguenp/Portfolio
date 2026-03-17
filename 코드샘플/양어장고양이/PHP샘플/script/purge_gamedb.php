<?php

if (!isset($argc) || 2 > $argc) {
    echo 'input confirmation key "Really" as first param' . PHP_EOL;
    exit;
}
if ('Really' !== $argv[1]) {
    echo 'confirmation key invalid' . PHP_EOL;
    exit;
}

require_once dirname(__FILE__) . '/../lib/common.php';

$DB_SCHEMA = 'h3_game';
$TEMP_SUFFIX = '_dupe';

try {
    $q_tb = "SELECT `TABLE_NAME` FROM `information_schema`.`TABLES` WHERE `TABLE_SCHEMA`='{$DB_SCHEMA}'";
    $res_tb = GameDb::queryFirstColumn($q_tb);
    if (!$res_tb || empty($res_tb)) {
        echo 'no tables to purge' . PHP_EOL;
    }

    GameDb::startTransaction();
    foreach($res_tb as $tb) {
        $tmp_tb = $tb . $TEMP_SUFFIX;
        GameDb::query("CREATE TABLE `{$tmp_tb}` LIKE `{$tb}`");
        GameDb::query("DROP TABLE IF EXISTS `{$tb}`");
        GameDb::query("ALTER TABLE `{$tmp_tb}` RENAME TO `{$tb}`");
    }
    GameDb::commit();
    
    echo strval(count($res_tb)) . ' tables purged' . PHP_EOL;
} catch (Exception $e) {
    echo 'exception: ' . $e->getMessage() . PHP_EOL;
}

// EOF
