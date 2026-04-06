from sqlalchemy import text

REVENUE_SQL = text("""
        WITH
        game1 AS (
            SELECT DATE(i.regist_at) AS dt,
                   SUM(i.price) AS daily_revenue,
                   COUNT(DISTINCT i.user_no) AS daily_payers
            FROM `mtd-game-1`.in_app_purchases i
            GROUP BY DATE(i.regist_at)
        ),
        game1_dau AS (
            SELECT DATE(FROM_UNIXTIME(received_at)) AS dt,
                   COUNT(DISTINCT user_no) AS daily_active_users
            FROM `mtd-game-1`.mail_box
            GROUP BY DATE(FROM_UNIXTIME(received_at))
        ),
        game1_nru AS (
            SELECT DATE(signup_at) AS dt,
                   COUNT(*) AS nru
            FROM `mtd-game-1`.user_base
            GROUP BY DATE(signup_at)
        ),

        game2 AS (
            SELECT DATE(i.regist_at) AS dt,
                   SUM(i.price) AS daily_revenue,
                   COUNT(DISTINCT i.user_no) AS daily_payers
            FROM `mtd-game-2`.in_app_purchases i
            GROUP BY DATE(i.regist_at)
        ),
        game2_dau AS (
            SELECT DATE(FROM_UNIXTIME(received_at)) AS dt,
                   COUNT(DISTINCT user_no) AS daily_active_users
            FROM `mtd-game-2`.mail_box
            GROUP BY DATE(FROM_UNIXTIME(received_at))
        ),
        game2_nru AS (
            SELECT DATE(signup_at) AS dt,
                   COUNT(*) AS nru
            FROM `mtd-game-2`.user_base
            GROUP BY DATE(signup_at)
        ),

        game3 AS (
            SELECT DATE(i.regist_at) AS dt,
                   SUM(i.price) AS daily_revenue,
                   COUNT(DISTINCT i.user_no) AS daily_payers
            FROM `mtd-game-3`.in_app_purchases i
            GROUP BY DATE(i.regist_at)
        ),
        game3_dau AS (
            SELECT DATE(FROM_UNIXTIME(received_at)) AS dt,
                   COUNT(DISTINCT user_no) AS daily_active_users
            FROM `mtd-game-3`.mail_box
            GROUP BY DATE(FROM_UNIXTIME(received_at))
        ),
        game3_nru AS (
            SELECT DATE(signup_at) AS dt,
                   COUNT(*) AS nru
            FROM `mtd-game-3`.user_base
            GROUP BY DATE(signup_at)
        ),

        all_dates AS (
            SELECT dt FROM game1
            UNION SELECT dt FROM game1_dau
            UNION SELECT dt FROM game1_nru
            UNION SELECT dt FROM game2
            UNION SELECT dt FROM game2_dau
            UNION SELECT dt FROM game2_nru
            UNION SELECT dt FROM game3
            UNION SELECT dt FROM game3_dau
            UNION SELECT dt FROM game3_nru
        )

        SELECT
            d.dt AS `날짜`,

            -- ===== 합계 =====
            IFNULL(g1.daily_revenue,0) + IFNULL(g2.daily_revenue,0) + IFNULL(g3.daily_revenue,0) AS `매출 합`,
            IFNULL(g1.daily_payers,0)  + IFNULL(g2.daily_payers,0)  + IFNULL(g3.daily_payers,0)  AS `PU 합`,
            IFNULL(d1.daily_active_users,0) + IFNULL(d2.daily_active_users,0) + IFNULL(d3.daily_active_users,0) AS `DAU 합`,
            IFNULL(n1.nru,0) + IFNULL(n2.nru,0) + IFNULL(n3.nru,0) AS `NRU 합`,

            -- ===== game1 =====
            IFNULL(g1.daily_revenue,0) AS `ANGEL 매출`,
            IFNULL(g1.daily_payers,0)  AS `ANGEL PU`,
            IFNULL(d1.daily_active_users,0) AS `ANGEL DAU`,
            IFNULL(n1.nru,0) AS `ANGEL NRU`,

            -- ===== game2 =====
            IFNULL(g2.daily_revenue,0) AS `WONDER 매출`,
            IFNULL(g2.daily_payers,0)  AS `WONDER PU`,
            IFNULL(d2.daily_active_users,0) AS `WONDER DAU`,
            IFNULL(n2.nru,0) AS `WONDER NRU`,

            -- ===== game3 =====
            IFNULL(g3.daily_revenue,0) AS `LUNA 매출`,
            IFNULL(g3.daily_payers,0)  AS `LUNA PU`,
            IFNULL(d3.daily_active_users,0) AS `LUNA DAU`,
            IFNULL(n3.nru,0) AS `LUNA NRU`

        FROM all_dates d
        LEFT JOIN game1 g1 ON g1.dt = d.dt
        LEFT JOIN game1_dau d1 ON d1.dt = d.dt
        LEFT JOIN game1_nru n1 ON n1.dt = d.dt
        LEFT JOIN game2 g2 ON g2.dt = d.dt
        LEFT JOIN game2_dau d2 ON d2.dt = d.dt
        LEFT JOIN game2_nru n2 ON n2.dt = d.dt
        LEFT JOIN game3 g3 ON g3.dt = d.dt
        LEFT JOIN game3_dau d3 ON d3.dt = d.dt
        LEFT JOIN game3_nru n3 ON n3.dt = d.dt    
        WHERE d.dt >= :start_date AND d.dt <= :end_date
        ORDER BY d.dt DESC
    """)

MAGNITE_SUMMARY_SQL = text("""
        SELECT
            server,

            /* Station */
            SUM(CASE WHEN station_type = 'Station' AND order_type = 1 THEN order_amount ELSE 0 END) AS station_mint,
            SUM(CASE WHEN station_type = 'Station' AND order_type = 2 THEN order_amount ELSE 0 END) AS station_burn,
            SUM(CASE WHEN station_type = 'Station' AND order_type = 1 THEN order_amount ELSE 0 END)
          - SUM(CASE WHEN station_type = 'Station' AND order_type = 2 THEN order_amount ELSE 0 END) AS station_diff,
          
            /* In-Game */
            SUM(CASE WHEN station_type = 'In-Game' AND order_type = 1 THEN order_amount ELSE 0 END) AS in_game_mint,
            SUM(CASE WHEN station_type = 'In-Game' AND order_type = 2 THEN order_amount ELSE 0 END) AS in_game_burn,
            SUM(CASE WHEN station_type = 'In-Game' AND order_type = 1 THEN order_amount ELSE 0 END)
          - SUM(CASE WHEN station_type = 'In-Game' AND order_type = 2 THEN order_amount ELSE 0 END) AS in_game_diff
        FROM (
            SELECT
                'angel' AS server,
                CASE 
                    WHEN order_desc LIKE '%Station%' THEN 'Station'
                    ELSE 'In-Game'
                END AS station_type,
                order_type,
                order_amount
            FROM `mtd-game-1`.user_magnite
            WHERE registed_at BETWEEN :start_date AND :end_date

            UNION ALL

            SELECT
                'wonder' AS server,
                CASE 
                    WHEN order_desc LIKE '%Station%' THEN 'Station'
                    ELSE 'In-Game'
                END AS station_type,
                order_type,
                order_amount
            FROM `mtd-game-2`.user_magnite
            WHERE registed_at BETWEEN :start_date AND :end_date

            UNION ALL

            SELECT
                'luna' AS server,
                CASE 
                    WHEN order_desc LIKE '%Station%' THEN 'Station'
                    ELSE 'In-Game'
                END AS station_type,
                order_type,
                order_amount
            FROM `mtd-game-3`.user_magnite
            WHERE registed_at BETWEEN :start_date AND :end_date
        ) t
        GROUP BY server
        ORDER BY FIELD(server, 'angel', 'wonder', 'luna')
    """)


MAGNITE_DETAIL_SQL = text("""
    SELECT
        server,
        registed_at,
        order_type,
        order_amount,
        order_desc
    FROM (
        SELECT
            'angel' AS server,
            registed_at,
            order_type,
            order_amount,
            order_desc
        FROM `mtd-game-1`.user_magnite

        UNION ALL

        SELECT
            'wonder' AS server,
            registed_at,
            order_type,
            order_amount,
            order_desc
        FROM `mtd-game-2`.user_magnite

        UNION ALL

        SELECT
            'luna' AS server,
            registed_at,
            order_type,
            order_amount,
            order_desc
        FROM `mtd-game-3`.user_magnite
    ) t
    WHERE registed_at BETWEEN :start_date AND :end_date
    ORDER BY registed_at DESC
    """)
    
USERS_REVENUE_SQL = text("""
SELECT
    server,
    user_no,
    nick,
    cnt,
    today_total_price,
    last_active_time,
    signup_at
FROM (
    SELECT 
        'angel' AS server,
        a.user_no,
        b.nick,
        COUNT(*) AS cnt,
        SUM(a.price) AS today_total_price,
        FROM_UNIXTIME(b.last_active_time) AS last_active_time,
        b.signup_at
    FROM `mtd-game-1`.in_app_purchases AS a
    LEFT JOIN `mtd-game-1`.user_base AS b
        ON a.user_no = b.user_no
    WHERE a.regist_at BETWEEN :start_date AND :end_date
    GROUP BY a.user_no, b.nick, b.last_active_time, b.signup_at

    UNION ALL

    SELECT 
        'wonder' AS server,
        a.user_no,
        b.nick,
        COUNT(*) AS cnt,
        SUM(a.price) AS today_total_price,
        FROM_UNIXTIME(b.last_active_time) AS last_active_time,
        b.signup_at
    FROM `mtd-game-2`.in_app_purchases AS a
    LEFT JOIN `mtd-game-2`.user_base AS b
        ON a.user_no = b.user_no
    WHERE a.regist_at BETWEEN :start_date AND :end_date
    GROUP BY a.user_no, b.nick, b.last_active_time, b.signup_at

    UNION ALL

    SELECT 
        'luna' AS server,
        a.user_no,
        b.nick,
        COUNT(*) AS cnt,
        SUM(a.price) AS today_total_price,
        FROM_UNIXTIME(b.last_active_time) AS last_active_time,
        b.signup_at
    FROM `mtd-game-3`.in_app_purchases AS a
    LEFT JOIN `mtd-game-3`.user_base AS b
        ON a.user_no = b.user_no
    WHERE a.regist_at BETWEEN :start_date AND :end_date
    GROUP BY a.user_no, b.nick, b.last_active_time, b.signup_at
) t
ORDER BY today_total_price DESC
""")