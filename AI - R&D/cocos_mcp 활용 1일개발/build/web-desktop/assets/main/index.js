System.register("chunks:///_virtual/Enemy.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './GameManager.ts', './PathFinder.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, _createClass, cclegacy, _decorator, Sprite, Color, Node, UITransform, Label, Graphics, Component, GameManager, canvasToGrid, findPath, grid, TILE, gridToCanvas, SPAWN_CELL;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
      _createClass = module.createClass;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Sprite = module.Sprite;
      Color = module.Color;
      Node = module.Node;
      UITransform = module.UITransform;
      Label = module.Label;
      Graphics = module.Graphics;
      Component = module.Component;
    }, function (module) {
      GameManager = module.GameManager;
    }, function (module) {
      canvasToGrid = module.canvasToGrid;
      findPath = module.findPath;
      grid = module.grid;
      TILE = module.TILE;
      gridToCanvas = module.gridToCanvas;
      SPAWN_CELL = module.SPAWN_CELL;
    }],
    execute: function () {
      var _ENEMY_TYPE_CFG, _dec, _class, _class2, _descriptor, _descriptor2, _descriptor3, _descriptor4, _class3;
      cclegacy._RF.push({}, "361eet7aKhJ/aVkNz/B4Vws", "Enemy", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;

      // ── Enemy type definitions ────────────────────────────────────────────────────

      var EnemyType = exports('EnemyType', /*#__PURE__*/function (EnemyType) {
        EnemyType[EnemyType["Basic"] = 0] = "Basic";
        EnemyType[EnemyType["Speed"] = 1] = "Speed";
        EnemyType[EnemyType["Tank"] = 2] = "Tank";
        return EnemyType;
      }({}));
      var ENEMY_TYPE_CFG = (_ENEMY_TYPE_CFG = {}, _ENEMY_TYPE_CFG[EnemyType.Basic] = {
        hpMult: 1.0,
        speedMult: 1.0,
        r: 220,
        g: 80,
        b: 80,
        scale: 1.0
      }, _ENEMY_TYPE_CFG[EnemyType.Speed] = {
        hpMult: 0.5,
        speedMult: 1.5,
        r: 80,
        g: 220,
        b: 220,
        scale: 0.75
      }, _ENEMY_TYPE_CFG[EnemyType.Tank] = {
        hpMult: 1.5,
        speedMult: 0.5,
        r: 180,
        g: 100,
        b: 30,
        scale: 1.3
      }, _ENEMY_TYPE_CFG);
      var Enemy = exports('Enemy', (_dec = ccclass('Enemy'), _dec(_class = (_class2 = (_class3 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(Enemy, _Component);
        function Enemy() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "maxHp", _descriptor, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "moveSpeed", _descriptor2, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "goldReward", _descriptor3, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "scoreReward", _descriptor4, _assertThisInitialized(_this));
          _this._hp = 100;
          _this._maxHpValue = 100;
          _this._wpIdx = 0;
          _this._dead = false;
          _this._path = [];
          _this._gridVer = -1;
          _this._slowFactor = 1.0;
          _this._slowTimer = 0;
          _this._isBoss = false;
          _this._baseR = 220;
          _this._baseG = 80;
          _this._baseB = 80;
          _this._flashing = false;
          _this._flashTimer = 0;
          _this._hpBarFill = null;
          return _this;
        }
        var _proto = Enemy.prototype;
        _proto.onEnable = function onEnable() {
          Enemy.activeEnemies.push(this);
        };
        _proto.onDisable = function onDisable() {
          var i = Enemy.activeEnemies.indexOf(this);
          if (i >= 0) Enemy.activeEnemies.splice(i, 1);
        }

        // ── Init ──────────────────────────────────────────────────────────────────────
        ;

        _proto.init = function init(hpMult, speedMult, isBoss, enemyType) {
          if (hpMult === void 0) {
            hpMult = 1;
          }
          if (speedMult === void 0) {
            speedMult = 1;
          }
          if (isBoss === void 0) {
            isBoss = false;
          }
          if (enemyType === void 0) {
            enemyType = EnemyType.Basic;
          }
          var typeCfg = ENEMY_TYPE_CFG[enemyType];
          this._maxHpValue = this.maxHp * hpMult * typeCfg.hpMult;
          this._hp = this._maxHpValue;
          this.moveSpeed = this.moveSpeed * speedMult * typeCfg.speedMult;
          this._dead = false;
          this._slowFactor = 1.0;
          this._slowTimer = 0;
          this._isBoss = isBoss;
          this._flashing = false;
          this._flashTimer = 0;
          this._hpBarFill = null;
          var spawn = gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row);
          this.node.setPosition(spawn.x, spawn.y, 0);
          this._recalcPath();
          if (isBoss) {
            this._baseR = 100;
            this._baseG = 0;
            this._baseB = 150;
            this.node.setScale(2, 2, 1);
            var sprite = this.node.getComponent(Sprite);
            if (sprite) sprite.color = new Color(100, 0, 150, 255);

            // BOSS label
            var bossLblNode = new Node('BossLbl');
            bossLblNode.layer = this.node.layer;
            bossLblNode.addComponent(UITransform).setContentSize(80, 22);
            var lbl = bossLblNode.addComponent(Label);
            lbl.string = 'BOSS';
            lbl.fontSize = 14;
            lbl.color = new Color(255, 50, 50, 255);
            bossLblNode.setPosition(0, 28, 0);
            bossLblNode.setScale(0.5, 0.5, 1);
            bossLblNode.setParent(this.node);
            this._createHPBar();
          } else {
            this._baseR = typeCfg.r;
            this._baseG = typeCfg.g;
            this._baseB = typeCfg.b;
            this.node.setScale(typeCfg.scale, typeCfg.scale, 1);
            var _sprite = this.node.getComponent(Sprite);
            if (_sprite) _sprite.color = new Color(typeCfg.r, typeCfg.g, typeCfg.b, 255);
            if (enemyType !== EnemyType.Basic) {
              var lblNode = new Node('TypeLbl');
              lblNode.layer = this.node.layer;
              lblNode.addComponent(UITransform).setContentSize(60, 16);
              var typeLbl = lblNode.addComponent(Label);
              typeLbl.string = enemyType === EnemyType.Speed ? 'SPD' : 'TNK';
              typeLbl.fontSize = 11;
              typeLbl.color = new Color(255, 255, 255, 200);
              lblNode.setPosition(0, 22, 0);
              lblNode.setScale(1 / typeCfg.scale, 1 / typeCfg.scale, 1);
              lblNode.setParent(this.node);
            }
          }
        }

        // ── Boss HP bar ───────────────────────────────────────────────────────────────
        ;

        _proto._createHPBar = function _createHPBar() {
          var barW = 120,
            barH = 10;
          var container = new Node('HPBarContainer');
          container.layer = this.node.layer;
          container.addComponent(UITransform).setContentSize(barW, barH);
          container.setPosition(0, 46, 0);
          container.setScale(0.5, 0.5, 1); // counter-scale boss 2×

          var bg = new Node('BG');
          bg.layer = this.node.layer;
          bg.addComponent(UITransform).setContentSize(barW, barH);
          var bgG = bg.addComponent(Graphics);
          bgG.fillColor = new Color(40, 0, 0, 220);
          bgG.rect(-barW / 2, -barH / 2, barW, barH);
          bgG.fill();
          bg.setParent(container);
          var fill = new Node('Fill');
          fill.layer = this.node.layer;
          fill.addComponent(UITransform).setContentSize(barW, barH);
          this._hpBarFill = fill;
          fill.setParent(container);
          container.setParent(this.node);
          this._updateHPBar();
        };
        _proto._updateHPBar = function _updateHPBar() {
          if (!this._hpBarFill || !this._hpBarFill.isValid) return;
          var ratio = Math.max(0, this._hp / this._maxHpValue);
          var barW = 120,
            barH = 10;
          var fillW = barW * ratio;
          var g = this._hpBarFill.getComponent(Graphics);
          if (!g) g = this._hpBarFill.addComponent(Graphics);
          g.clear();
          // Color: green → yellow → red as HP drops
          var r = ratio > 0.5 ? Math.floor(255 * (1 - ratio) * 2) : 255;
          var gn = ratio > 0.5 ? 200 : Math.floor(200 * ratio * 2);
          g.fillColor = new Color(r, gn, 0, 255);
          g.rect(-barW / 2, -barH / 2, fillW, barH);
          g.fill();
        }

        // ── Slow & color ──────────────────────────────────────────────────────────────
        ;

        _proto.applySlow = function applySlow(toFactor, duration) {
          var wasSlowed = this._slowTimer > 0;
          this._slowFactor = Math.min(this._slowFactor, toFactor);
          this._slowTimer = Math.max(this._slowTimer, duration);
          if (!wasSlowed && !this._flashing) {
            this._applyColor();
          }
        }

        /** Set sprite color based on current state (slow / normal), skip during flash */;
        _proto._applyColor = function _applyColor() {
          if (this._isBoss) return;
          var sprite = this.node.getComponent(Sprite);
          if (!sprite) return;
          if (this._slowTimer > 0) {
            // Blue-tinted — set once when slow starts, cleared when slow ends
            sprite.color = new Color(Math.floor(this._baseR * 0.4), Math.floor(Math.min(255, this._baseG * 0.4 + 100)), Math.min(255, Math.floor(this._baseB * 0.4 + 160)), 255);
          } else {
            sprite.color = new Color(this._baseR, this._baseG, this._baseB, 255);
          }
        }

        // ── Damage ────────────────────────────────────────────────────────────────────
        ;

        _proto.takeDamage = function takeDamage(dmg) {
          if (this._dead) return;
          this._hp -= dmg;
          // White flash
          var sprite = this.node.getComponent(Sprite);
          if (sprite) sprite.color = new Color(255, 255, 255, 255);
          this._flashing = true;
          this._flashTimer = Enemy.FLASH_DUR;
          this._updateHPBar();
          if (this._hp <= 0) this._die();
        };
        _proto._die = function _die() {
          var _GameManager$instance, _GameManager$instance2;
          if (this._dead) return;
          this._dead = true;
          (_GameManager$instance = GameManager.instance) == null || _GameManager$instance.addGold(this.goldReward);
          (_GameManager$instance2 = GameManager.instance) == null || _GameManager$instance2.addScore(this.scoreReward);
          this.node.destroy();
        }

        // ── Pathfinding ───────────────────────────────────────────────────────────────
        ;

        _proto._recalcPath = function _recalcPath() {
          var pos = this.node.getPosition();
          var _canvasToGrid = canvasToGrid(pos.x, pos.y),
            col = _canvasToGrid.col,
            row = _canvasToGrid.row;
          var newPath = findPath(col, row, grid.blocked);
          if (newPath && newPath.length > 0) {
            this._path = newPath;
            this._wpIdx = 0;
            if (newPath.length > 1) {
              var wp0 = newPath[0];
              var dx = wp0.x - pos.x;
              var dy = wp0.y - pos.y;
              if (dx * dx + dy * dy < Math.pow(TILE * 0.5, 2)) this._wpIdx = 1;
            }
          }
          this._gridVer = grid.version;
        }

        // ── Update ────────────────────────────────────────────────────────────────────
        ;

        _proto.update = function update(dt) {
          var _GameManager$instance3, _GameManager$instance4;
          if (this._dead) return;
          var ts = (_GameManager$instance3 = (_GameManager$instance4 = GameManager.instance) == null ? void 0 : _GameManager$instance4.timeScale) != null ? _GameManager$instance3 : 1;
          if (ts === 0) return;
          var edt = dt * ts;

          // Hit flash
          if (this._flashing) {
            this._flashTimer -= edt;
            if (this._flashTimer <= 0) {
              this._flashing = false;
              this._applyColor();
            }
          }

          // Slow timer
          if (this._slowTimer > 0) {
            this._slowTimer -= edt;
            if (this._slowTimer <= 0) {
              this._slowTimer = 0;
              this._slowFactor = 1.0;
              if (!this._flashing) this._applyColor();
            }
          }
          if (this._gridVer !== grid.version) this._recalcPath();
          if (this._wpIdx >= this._path.length) {
            var _GameManager$instance5;
            (_GameManager$instance5 = GameManager.instance) == null || _GameManager$instance5.loseLife();
            this._dead = true;
            this.node.destroy();
            return;
          }
          var wp = this._path[this._wpIdx];
          var pos = this.node.getPosition();
          var dx = wp.x - pos.x;
          var dy = wp.y - pos.y;
          var len = Math.sqrt(dx * dx + dy * dy);
          if (len < 5) {
            this._wpIdx++;
            return;
          }
          var spd = this.moveSpeed * this._slowFactor * edt / len;
          this.node.setPosition(pos.x + dx * spd, pos.y + dy * spd, 0);
        };
        _createClass(Enemy, [{
          key: "waypointProgress",
          get: function get() {
            return this._wpIdx;
          }
        }, {
          key: "isDead",
          get: function get() {
            return this._dead;
          }
        }]);
        return Enemy;
      }(Component), _class3.activeEnemies = [], _class3.FLASH_DUR = 0.08, _class3), (_descriptor = _applyDecoratedDescriptor(_class2.prototype, "maxHp", [property], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return 100;
        }
      }), _descriptor2 = _applyDecoratedDescriptor(_class2.prototype, "moveSpeed", [property], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return 100;
        }
      }), _descriptor3 = _applyDecoratedDescriptor(_class2.prototype, "goldReward", [property], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return 20;
        }
      }), _descriptor4 = _applyDecoratedDescriptor(_class2.prototype, "scoreReward", [property], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return 10;
        }
      })), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/EnemySpawner.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './Enemy.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, cclegacy, _decorator, Prefab, Node, instantiate, Component, EnemyType, Enemy;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Prefab = module.Prefab;
      Node = module.Node;
      instantiate = module.instantiate;
      Component = module.Component;
    }, function (module) {
      EnemyType = module.EnemyType;
      Enemy = module.Enemy;
    }],
    execute: function () {
      var _dec, _dec2, _dec3, _class, _class2, _descriptor, _descriptor2;
      cclegacy._RF.push({}, "5dcacD+8cJO8JlkOxzlghq3", "EnemySpawner", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;
      var EnemySpawner = exports('EnemySpawner', (_dec = ccclass('EnemySpawner'), _dec2 = property(Prefab), _dec3 = property(Node), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(EnemySpawner, _Component);
        function EnemySpawner() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "enemyPrefab", _descriptor, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "enemyContainer", _descriptor2, _assertThisInitialized(_this));
          return _this;
        }
        var _proto = EnemySpawner.prototype;
        _proto.spawn = function spawn(hpMult, speedMult, type) {
          if (hpMult === void 0) {
            hpMult = 1;
          }
          if (speedMult === void 0) {
            speedMult = 1;
          }
          if (type === void 0) {
            type = EnemyType.Basic;
          }
          if (!this.enemyPrefab || !this.enemyContainer) return null;
          var node = instantiate(this.enemyPrefab);
          this.enemyContainer.addChild(node);
          var enemy = node.getComponent(Enemy);
          enemy == null || enemy.init(hpMult, speedMult, false, type);
          return enemy;
        };
        _proto.spawnBoss = function spawnBoss(hpMult, speedMult) {
          if (!this.enemyPrefab || !this.enemyContainer) return null;
          var node = instantiate(this.enemyPrefab);
          this.enemyContainer.addChild(node);
          var enemy = node.getComponent(Enemy);
          enemy == null || enemy.init(hpMult, speedMult, true);
          return enemy;
        };
        return EnemySpawner;
      }(Component), (_descriptor = _applyDecoratedDescriptor(_class2.prototype, "enemyPrefab", [_dec2], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _descriptor2 = _applyDecoratedDescriptor(_class2.prototype, "enemyContainer", [_dec3], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      })), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/GameManager.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc'], function (exports) {
  var _inheritsLoose, cclegacy, _decorator, director, Component;
  return {
    setters: [function (module) {
      _inheritsLoose = module.inheritsLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      director = module.director;
      Component = module.Component;
    }],
    execute: function () {
      var _dec, _class, _class2;
      cclegacy._RF.push({}, "4f0acW8BUlF6og3K0zTzBjW", "GameManager", undefined);
      var ccclass = _decorator.ccclass;
      var GameState = exports('GameState', /*#__PURE__*/function (GameState) {
        GameState["IDLE"] = "IDLE";
        GameState["PLAYING"] = "PLAYING";
        GameState["PAUSED"] = "PAUSED";
        GameState["GAMEOVER"] = "GAMEOVER";
        return GameState;
      }({}));
      var GameManager = exports('GameManager', (_dec = ccclass('GameManager'), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(GameManager, _Component);
        function GameManager() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _this.gold = 200;
          _this.lives = 20;
          _this.score = 0;
          _this.wave = 0;
          _this.state = GameState.IDLE;
          _this.speed = 1;
          // 1 or 2
          _this.timeScale = 1;
          return _this;
        }
        var _proto = GameManager.prototype;
        // 0=paused, 1=normal, 2=2x  — read by all update() loops
        _proto.onLoad = function onLoad() {
          if (GameManager.instance) {
            this.destroy();
            return;
          }
          GameManager.instance = this;
          director.addPersistRootNode(this.node);
        };
        _proto.onDestroy = function onDestroy() {
          if (GameManager.instance === this) GameManager.instance = null;
        };
        _proto.reset = function reset() {
          this.gold = 200;
          this.lives = 20;
          this.score = 0;
          this.wave = 0;
          this.speed = 1;
          this.timeScale = 1;
          this.state = GameState.PLAYING;
        };
        _proto.spendGold = function spendGold(n) {
          if (this.gold < n) return false;
          this.gold -= n;
          return true;
        };
        _proto.addGold = function addGold(n) {
          this.gold += n;
        };
        _proto.addScore = function addScore(n) {
          this.score += n;
        };
        _proto.pause = function pause() {
          if (this.state !== GameState.PLAYING) return;
          this.state = GameState.PAUSED;
          this.timeScale = 0;
        };
        _proto.resume = function resume() {
          if (this.state !== GameState.PAUSED) return;
          this.state = GameState.PLAYING;
          this.timeScale = this.speed;
        };
        _proto.setSpeed = function setSpeed(s) {
          this.speed = s;
          if (this.state === GameState.PLAYING) {
            this.timeScale = s;
          }
        };
        _proto.loseLife = function loseLife() {
          this.lives--;
          if (this.lives <= 0) {
            this.state = GameState.GAMEOVER;
            this.timeScale = 1;
            director.loadScene('GameOver');
          }
        };
        return GameManager;
      }(Component), _class2.instance = null, _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/GameOverUI.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './GameManager.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, cclegacy, _decorator, Label, Button, director, Component, GameManager;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Label = module.Label;
      Button = module.Button;
      director = module.director;
      Component = module.Component;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _dec2, _class, _class2, _descriptor;
      cclegacy._RF.push({}, "e990avPKEFGWqDXF9vgVrqD", "GameOverUI", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;
      var GameOverUI = exports('GameOverUI', (_dec = ccclass('GameOverUI'), _dec2 = property(Label), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(GameOverUI, _Component);
        function GameOverUI() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "finalScoreLabel", _descriptor, _assertThisInitialized(_this));
          return _this;
        }
        var _proto = GameOverUI.prototype;
        _proto.start = function start() {
          // Show score
          var gm = GameManager.instance;
          if (this.finalScoreLabel && gm) {
            this.finalScoreLabel.string = "Final Score: " + gm.score;
          }

          // Register buttons programmatically
          var canvas = this.node.parent;
          var restartBtn = canvas == null ? void 0 : canvas.getChildByName('RestartBtn');
          var menuBtn = canvas == null ? void 0 : canvas.getChildByName('MainMenuBtn');
          if (restartBtn) {
            restartBtn.on(Button.EventType.CLICK, this._onRestartClick, this);
            console.log('[GameOverUI] RestartBtn registered');
          }
          if (menuBtn) {
            menuBtn.on(Button.EventType.CLICK, this._onMainMenuClick, this);
            console.log('[GameOverUI] MainMenuBtn registered');
          }
        };
        _proto._onRestartClick = function _onRestartClick() {
          var _GameManager$instance;
          (_GameManager$instance = GameManager.instance) == null || _GameManager$instance.reset();
          director.loadScene('GameScene');
        };
        _proto._onMainMenuClick = function _onMainMenuClick() {
          director.loadScene('MainMenu');
        }

        // Keep for scene-based clickEvents
        ;

        _proto.onRestartClick = function onRestartClick() {
          this._onRestartClick();
        };
        _proto.onMainMenuClick = function onMainMenuClick() {
          this._onMainMenuClick();
        };
        return GameOverUI;
      }(Component), _descriptor = _applyDecoratedDescriptor(_class2.prototype, "finalScoreLabel", [_dec2], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/main", ['./Enemy.ts', './EnemySpawner.ts', './GameManager.ts', './GameOverUI.ts', './MainMenuUI.ts', './PathData.ts', './PathFinder.ts', './Projectile.ts', './Tower.ts', './TowerData.ts', './TowerManager.ts', './UIManager.ts', './WaveManager.ts'], function () {
  return {
    setters: [null, null, null, null, null, null, null, null, null, null, null, null, null],
    execute: function () {}
  };
});

System.register("chunks:///_virtual/MainMenuUI.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './GameManager.ts'], function (exports) {
  var _inheritsLoose, cclegacy, _decorator, Button, director, Component, GameManager;
  return {
    setters: [function (module) {
      _inheritsLoose = module.inheritsLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Button = module.Button;
      director = module.director;
      Component = module.Component;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _class;
      cclegacy._RF.push({}, "1c7bcOC1bJJaqnwhk7O4SRJ", "MainMenuUI", undefined);
      var ccclass = _decorator.ccclass;
      var MainMenuUI = exports('MainMenuUI', (_dec = ccclass('MainMenuUI'), _dec(_class = /*#__PURE__*/function (_Component) {
        _inheritsLoose(MainMenuUI, _Component);
        function MainMenuUI() {
          return _Component.apply(this, arguments) || this;
        }
        var _proto = MainMenuUI.prototype;
        _proto.start = function start() {
          // Register button click programmatically — most reliable approach
          var canvas = this.node.parent;
          var startBtn = canvas == null ? void 0 : canvas.getChildByName('StartBtn');
          if (startBtn) {
            startBtn.on(Button.EventType.CLICK, this._onStartClick, this);
            console.log('[MainMenuUI] StartBtn click registered');
          } else {
            console.warn('[MainMenuUI] StartBtn not found!');
          }
        };
        _proto._onStartClick = function _onStartClick() {
          var _GameManager$instance;
          (_GameManager$instance = GameManager.instance) == null || _GameManager$instance.reset();
          director.loadScene('GameScene');
        }

        // Keep this for any scene-based clickEvent that may exist
        ;

        _proto.onStartClick = function onStartClick() {
          this._onStartClick();
        };
        return MainMenuUI;
      }(Component)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/PathData.ts", ['cc'], function (exports) {
  var cclegacy;
  return {
    setters: [function (module) {
      cclegacy = module.cclegacy;
    }],
    execute: function () {
      cclegacy._RF.push({}, "4d6d1lCknJCp5Ij/XKyKVJN", "PathData", undefined);
      var PATH_WAYPOINTS = exports('PATH_WAYPOINTS', [{
        x: -512,
        y: 256
      }, {
        x: 192,
        y: 256
      }, {
        x: 192,
        y: 64
      }, {
        x: -192,
        y: 64
      }, {
        x: -192,
        y: -192
      }, {
        x: 512,
        y: -192
      }]);
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/PathFinder.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc'], function (exports) {
  var _createForOfIteratorHelperLoose, cclegacy;
  return {
    setters: [function (module) {
      _createForOfIteratorHelperLoose = module.createForOfIteratorHelperLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
    }],
    execute: function () {
      exports({
        canvasToGrid: canvasToGrid,
        cellKey: cellKey,
        findPath: findPath,
        gridToCanvas: gridToCanvas,
        isPathPossible: isPathPossible
      });
      cclegacy._RF.push({}, "60cfdFuN4ND75KdHSQdGHVT", "PathFinder", undefined);
      // ── Tile grid constants ────────────────────────────────────────────────────────
      var GRID_W = exports('GRID_W', 20); // 1280 / 64
      var GRID_H = exports('GRID_H', 11); // 704  / 64  (bottom 16px unused)
      var TILE = exports('TILE', 64);

      // Enemy enters at left-edge, exits at right-edge
      var SPAWN_CELL = exports('SPAWN_CELL', {
        col: 0,
        row: 8
      }); // canvas-local ≈ (-608, 168)
      var EXIT_CELL = exports('EXIT_CELL', {
        col: 19,
        row: 2
      }); // canvas-local ≈ (608, -200)

      // ── Shared mutable grid state (read by Enemy, written by TowerManager) ─────────
      var grid = exports('grid', {
        blocked: new Set(),
        // "col,row" keys of tower-occupied cells
        version: 0 // increments every time towers change
      });

      // ── Helpers ────────────────────────────────────────────────────────────────────
      function cellKey(col, row) {
        return col + "," + row;
      }
      function gridToCanvas(col, row) {
        return {
          x: -640 + col * TILE + TILE * 0.5,
          y: -360 + row * TILE + TILE * 0.5
        };
      }
      function canvasToGrid(x, y) {
        return {
          col: Math.max(0, Math.min(GRID_W - 1, Math.floor((x + 640) / TILE))),
          row: Math.max(0, Math.min(GRID_H - 1, Math.floor((y + 360) / TILE)))
        };
      }

      // ── A* from (startCol, startRow) to EXIT_CELL ──────────────────────────────────
      var DIRS = [[1, 0], [-1, 0], [0, 1], [0, -1]];
      function findPath(startCol, startRow, blocked) {
        var ec = EXIT_CELL.col;
        var er = EXIT_CELL.row;
        if (startCol === ec && startRow === er) {
          return [gridToCanvas(ec, er)];
        }
        var h = function h(c, r) {
          return Math.abs(c - ec) + Math.abs(r - er);
        };
        var openSet = new Set();
        var cameFrom = new Map();
        var gScore = new Map();
        var fScore = new Map();
        var sk = cellKey(startCol, startRow);
        openSet.add(sk);
        gScore.set(sk, 0);
        fScore.set(sk, h(startCol, startRow));
        while (openSet.size > 0) {
          // Pick node with lowest fScore
          var curKey = '';
          var curF = Infinity;
          for (var _iterator = _createForOfIteratorHelperLoose(openSet), _step; !(_step = _iterator()).done;) {
            var _fScore$get;
            var _k = _step.value;
            var f = (_fScore$get = fScore.get(_k)) != null ? _fScore$get : Infinity;
            if (f < curF) {
              curF = f;
              curKey = _k;
            }
          }
          var parts = curKey.split(',');
          var cc = +parts[0];
          var cr = +parts[1];
          if (cc === ec && cr === er) {
            // Reconstruct path
            var path = [];
            var k = curKey;
            while (cameFrom.has(k)) {
              var _k$split$map = k.split(',').map(Number),
                c = _k$split$map[0],
                r = _k$split$map[1];
              path.unshift(gridToCanvas(c, r));
              k = cameFrom.get(k);
            }
            path.unshift(gridToCanvas(startCol, startRow));
            return path;
          }
          openSet["delete"](curKey);
          for (var _iterator2 = _createForOfIteratorHelperLoose(DIRS), _step2; !(_step2 = _iterator2()).done;) {
            var _gScore$get, _gScore$get2;
            var _step2$value = _step2.value,
              dc = _step2$value[0],
              dr = _step2$value[1];
            var nc = cc + dc;
            var nr = cr + dr;
            if (nc < 0 || nc >= GRID_W || nr < 0 || nr >= GRID_H) continue;
            var nk = cellKey(nc, nr);
            if (blocked.has(nk)) continue;
            var tg = ((_gScore$get = gScore.get(curKey)) != null ? _gScore$get : 0) + 1;
            if (tg < ((_gScore$get2 = gScore.get(nk)) != null ? _gScore$get2 : Infinity)) {
              cameFrom.set(nk, curKey);
              gScore.set(nk, tg);
              fScore.set(nk, tg + h(nc, nr));
              openSet.add(nk);
            }
          }
        }
        return null; // No path exists
      }

      // Quickly check if SPAWN → EXIT is reachable with the given blocked set
      function isPathPossible(blocked) {
        return findPath(SPAWN_CELL.col, SPAWN_CELL.row, blocked) !== null;
      }
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/Projectile.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './Enemy.ts', './GameManager.ts'], function (exports) {
  var _inheritsLoose, _createForOfIteratorHelperLoose, cclegacy, _decorator, Component, Enemy, GameManager;
  return {
    setters: [function (module) {
      _inheritsLoose = module.inheritsLoose;
      _createForOfIteratorHelperLoose = module.createForOfIteratorHelperLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Component = module.Component;
    }, function (module) {
      Enemy = module.Enemy;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _class;
      cclegacy._RF.push({}, "c46b1VEQwJAYbiaPswV18c9", "Projectile", undefined);
      var ccclass = _decorator.ccclass;
      var Projectile = exports('Projectile', (_dec = ccclass('Projectile'), _dec(_class = /*#__PURE__*/function (_Component) {
        _inheritsLoose(Projectile, _Component);
        function Projectile() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _this._targetEnemy = null;
          _this._targetX = 0;
          _this._targetY = 0;
          _this._speed = 700;
          _this._damage = 0;
          _this._splashRadius = 0;
          _this._slowFactor = 0;
          _this._slowDuration = 0;
          // Arc (splash tower)
          _this._useArc = false;
          _this._startX = 0;
          _this._startY = 0;
          _this._arcDuration = 0;
          _this._arcElapsed = 0;
          _this._arcHeight = 0;
          return _this;
        }
        var _proto = Projectile.prototype;
        _proto.init = function init(target, damage, splashRadius, slowFactor, slowDuration, useArc) {
          if (splashRadius === void 0) {
            splashRadius = 0;
          }
          if (slowFactor === void 0) {
            slowFactor = 0;
          }
          if (slowDuration === void 0) {
            slowDuration = 0;
          }
          if (useArc === void 0) {
            useArc = false;
          }
          this._targetEnemy = target;
          this._damage = damage;
          this._splashRadius = splashRadius;
          this._slowFactor = slowFactor;
          this._slowDuration = slowDuration;
          this._useArc = useArc;

          // Capture target world position at fire time
          var tp = target.node.getWorldPosition();
          this._targetX = tp.x;
          this._targetY = tp.y;
          if (useArc) {
            var sp = this.node.getWorldPosition();
            this._startX = sp.x;
            this._startY = sp.y;
            var dist = Math.sqrt(Math.pow(this._targetX - sp.x, 2) + Math.pow(this._targetY - sp.y, 2));
            this._arcDuration = Math.max(0.3, dist / this._speed);
            this._arcElapsed = 0;
            this._arcHeight = Math.min(160, dist * 0.45);
          }
        };
        _proto.update = function update(dt) {
          var _GameManager$instance, _GameManager$instance2;
          var ts = (_GameManager$instance = (_GameManager$instance2 = GameManager.instance) == null ? void 0 : _GameManager$instance2.timeScale) != null ? _GameManager$instance : 1;
          if (ts === 0) return;
          var edt = dt * ts;
          if (this._useArc) {
            this._updateArc(edt);
          } else {
            this._updateStraight(edt);
          }
        };
        _proto._updateStraight = function _updateStraight(edt) {
          // Non-splash: abort if target already dead
          if (this._splashRadius === 0) {
            var t = this._targetEnemy;
            if (!t || !t.isValid || t.isDead) {
              this.node.destroy();
              return;
            }
          }
          var mp = this.node.getWorldPosition();
          var dx = this._targetX - mp.x;
          var dy = this._targetY - mp.y;
          var len = Math.sqrt(dx * dx + dy * dy);
          if (len < 10) {
            this._onHit();
            this.node.destroy();
            return;
          }
          var s = this._speed * edt / len;
          this.node.setWorldPosition(mp.x + dx * s, mp.y + dy * s, 0);
        };
        _proto._updateArc = function _updateArc(edt) {
          this._arcElapsed += edt;
          var t = Math.min(this._arcElapsed / this._arcDuration, 1);
          var ix = this._startX + (this._targetX - this._startX) * t;
          var iy = this._startY + (this._targetY - this._startY) * t;
          var arc = this._arcHeight * 4 * t * (1 - t); // parabola: 0 at both ends
          this.node.setWorldPosition(ix, iy + arc, 0);
          if (t >= 1) {
            this._onHit();
            this.node.destroy();
          }
        };
        _proto._onHit = function _onHit() {
          if (this._splashRadius > 0) {
            for (var _iterator = _createForOfIteratorHelperLoose(Enemy.activeEnemies.slice()), _step; !(_step = _iterator()).done;) {
              var e = _step.value;
              if (!e || !e.isValid || e.isDead) continue;
              var ep = e.node.getWorldPosition();
              var dx = ep.x - this._targetX;
              var dy = ep.y - this._targetY;
              if (Math.sqrt(dx * dx + dy * dy) <= this._splashRadius) {
                if (this._damage > 0) e.takeDamage(this._damage);
                if (this._slowFactor > 0) e.applySlow(this._slowFactor, this._slowDuration);
              }
            }
          } else {
            var t = this._targetEnemy;
            if (t && t.isValid && !t.isDead) {
              if (this._damage > 0) t.takeDamage(this._damage);
              if (this._slowFactor > 0) t.applySlow(this._slowFactor, this._slowDuration);
            }
          }
        };
        return Projectile;
      }(Component)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/Tower.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './TowerData.ts', './Enemy.ts', './Projectile.ts', './GameManager.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, _createForOfIteratorHelperLoose, cclegacy, _decorator, Prefab, Node, UITransform, Graphics, Color, Sprite, Label, Vec3, instantiate, Component, TowerType, TOWER_CONFIGS, getStats, Enemy, Projectile, GameManager;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
      _createForOfIteratorHelperLoose = module.createForOfIteratorHelperLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Prefab = module.Prefab;
      Node = module.Node;
      UITransform = module.UITransform;
      Graphics = module.Graphics;
      Color = module.Color;
      Sprite = module.Sprite;
      Label = module.Label;
      Vec3 = module.Vec3;
      instantiate = module.instantiate;
      Component = module.Component;
    }, function (module) {
      TowerType = module.TowerType;
      TOWER_CONFIGS = module.TOWER_CONFIGS;
      getStats = module.getStats;
    }, function (module) {
      Enemy = module.Enemy;
    }, function (module) {
      Projectile = module.Projectile;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _dec2, _class, _class2, _descriptor;
      cclegacy._RF.push({}, "631eeYnv/tCwovUFKNXA8H1", "Tower", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;

      // Towers that deal damage instantly (no visible projectile)
      var DIRECT_HIT_TYPES = new Set([TowerType.Normal, TowerType.Rapid, TowerType.Sniper]);
      var Tower = exports('Tower', (_dec = ccclass('Tower'), _dec2 = property(Prefab), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(Tower, _Component);
        function Tower() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "projectilePrefab", _descriptor, _assertThisInitialized(_this));
          _this.towerType = TowerType.Normal;
          _this.level = 1;
          _this._stats = {
            damage: 30,
            fireRate: 1.0,
            range: 160,
            splashRadius: 0,
            slowFactor: 0,
            slowDuration: 0
          };
          _this._cooldown = 0;
          _this._levelLabel = null;
          _this._barrel = null;
          _this._rangeCircle = null;
          return _this;
        }
        var _proto = Tower.prototype;
        // ── Public API ────────────────────────────────────────────────────────────────
        _proto.init = function init(type) {
          this.towerType = type;
          this.level = 1;
          this._refresh();
          this._updateVisual();
          this._createBarrel();
        };
        _proto.upgrade = function upgrade() {
          if (this.level >= 10) return false;
          this.level++;
          this._refresh();
          this._updateVisual();
          return true;
        };
        _proto.showRangeCircle = function showRangeCircle() {
          this.hideRangeCircle();
          var n = new Node('RangeCircle');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(this._stats.range * 2, this._stats.range * 2);
          var g = n.addComponent(Graphics);
          var _TOWER_CONFIGS$this$t = TOWER_CONFIGS[this.towerType].color,
            r = _TOWER_CONFIGS$this$t[0],
            gb = _TOWER_CONFIGS$this$t[1],
            b = _TOWER_CONFIGS$this$t[2];
          g.strokeColor = new Color(r, gb, b, 120);
          g.lineWidth = 2;
          g.circle(0, 0, this._stats.range);
          g.stroke();
          g.fillColor = new Color(r, gb, b, 20);
          g.circle(0, 0, this._stats.range);
          g.fill();
          n.setParent(this.node);
          this._rangeCircle = n;
        };
        _proto.hideRangeCircle = function hideRangeCircle() {
          var _this$_rangeCircle;
          (_this$_rangeCircle = this._rangeCircle) == null || _this$_rangeCircle.destroy();
          this._rangeCircle = null;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────
        ;

        _proto._refresh = function _refresh() {
          this._stats = getStats(this.towerType, this.level);
        };
        _proto._updateVisual = function _updateVisual() {
          var cfg = TOWER_CONFIGS[this.towerType];
          var _cfg$color = cfg.color,
            r = _cfg$color[0],
            g = _cfg$color[1],
            b = _cfg$color[2];
          var sprite = this.node.getComponent(Sprite);
          if (sprite) sprite.color = new Color(r, g, b, 255);
          var tr = this.node.getComponent(UITransform);
          if (tr) tr.setContentSize(cfg.size, cfg.size);
          if (!this._levelLabel) {
            var labelNode = new Node('LevelLabel');
            labelNode.layer = this.node.layer;
            labelNode.addComponent(UITransform).setContentSize(60, 20);
            var lbl = labelNode.addComponent(Label);
            lbl.fontSize = 14;
            lbl.color = new Color(255, 255, 255, 255);
            this._levelLabel = lbl;
            labelNode.setPosition(new Vec3(0, cfg.size * 0.5 + 10, 0));
            labelNode.setParent(this.node);
          } else {
            this._levelLabel.node.setPosition(new Vec3(0, cfg.size * 0.5 + 10, 0));
          }
          this._levelLabel.string = "Lv." + this.level;
        }

        /** Rotating barrel pointing right by default */;
        _proto._createBarrel = function _createBarrel() {
          if (this._barrel) {
            this._barrel.destroy();
          }
          var cfg = TOWER_CONFIGS[this.towerType];
          var bLen = cfg.size * 0.65;
          var bWidth = 6;
          var n = new Node('Barrel');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(bLen, bWidth);
          var g = n.addComponent(Graphics);
          var _cfg$color2 = cfg.color,
            r = _cfg$color2[0],
            gb = _cfg$color2[1],
            b = _cfg$color2[2];
          g.fillColor = new Color(Math.max(0, r - 50), Math.max(0, gb - 50), Math.max(0, b - 50), 255);
          // Barrel starts at origin, extends rightward
          g.rect(0, -bWidth / 2, bLen, bWidth);
          g.fill();

          // Darker end cap
          g.fillColor = new Color(Math.max(0, r - 80), Math.max(0, gb - 80), Math.max(0, b - 80), 255);
          g.rect(bLen - 4, -bWidth / 2, 4, bWidth);
          g.fill();
          n.setPosition(Vec3.ZERO);
          n.setParent(this.node);
          this._barrel = n;
        };
        _proto._findTarget = function _findTarget() {
          var pos = this.node.getWorldPosition();
          var best = null;
          var bestProgress = -1;
          for (var _iterator = _createForOfIteratorHelperLoose(Enemy.activeEnemies), _step; !(_step = _iterator()).done;) {
            var e = _step.value;
            if (!e || !e.isValid || e.isDead) continue;
            var ep = e.node.getWorldPosition();
            var dx = ep.x - pos.x;
            var dy = ep.y - pos.y;
            if (Math.sqrt(dx * dx + dy * dy) <= this._stats.range) {
              var p = e.waypointProgress;
              if (p > bestProgress) {
                bestProgress = p;
                best = e;
              }
            }
          }
          return best;
        };
        _proto._shoot = function _shoot(target) {
          var _this$node$parent;
          var cfg = TOWER_CONFIGS[this.towerType];
          var _cfg$color3 = cfg.color,
            r = _cfg$color3[0],
            gb = _cfg$color3[1],
            b = _cfg$color3[2];
          if (DIRECT_HIT_TYPES.has(this.towerType)) {
            // ── Instant hit: apply damage + hit flash at enemy position ──────────
            if (this._stats.damage > 0) target.takeDamage(this._stats.damage);
            if (this._stats.slowFactor > 0) target.applySlow(this._stats.slowFactor, this._stats.slowDuration);
            this._spawnHitFlash(target, r, gb, b);
            return;
          }

          // ── Projectile-based towers ───────────────────────────────────────────────
          if (!this.projectilePrefab) return;
          var node = instantiate(this.projectilePrefab);
          (_this$node$parent = this.node.parent) == null || _this$node$parent.addChild(node);
          node.setWorldPosition(this.node.getWorldPosition());
          var proj = node.getComponent(Projectile);
          if (proj) {
            proj.init(target, this._stats.damage, this._stats.splashRadius, this._stats.slowFactor, this._stats.slowDuration, this.towerType === TowerType.Splash // arc for splash
            );
          }

          var sprite = node.getComponent(Sprite);
          if (sprite) sprite.color = new Color(r, gb, b, 255);
        }

        /** Tiny colored circle flash at the enemy's position */;
        _proto._spawnHitFlash = function _spawnHitFlash(target, r, gb, b) {
          var _this$node$parent2;
          var flash = new Node('HitFlash');
          flash.layer = this.node.layer;
          flash.addComponent(UITransform).setContentSize(20, 20);
          flash.setWorldPosition(target.node.getWorldPosition());
          var g = flash.addComponent(Graphics);
          g.fillColor = new Color(r, gb, b, 200);
          g.circle(0, 0, 9);
          g.fill();
          g.strokeColor = new Color(255, 255, 255, 180);
          g.lineWidth = 2;
          g.circle(0, 0, 9);
          g.stroke();
          (_this$node$parent2 = this.node.parent) == null || _this$node$parent2.addChild(flash);
          this.scheduleOnce(function () {
            if (flash.isValid) flash.destroy();
          }, 0.07);
        }

        // ── Update ────────────────────────────────────────────────────────────────────
        ;

        _proto.update = function update(dt) {
          var _GameManager$instance, _GameManager$instance2;
          var ts = (_GameManager$instance = (_GameManager$instance2 = GameManager.instance) == null ? void 0 : _GameManager$instance2.timeScale) != null ? _GameManager$instance : 1;
          if (ts === 0) return;
          var edt = dt * ts;
          this._cooldown -= edt;
          var target = this._findTarget();

          // Aim barrel at target (or keep last angle if no target)
          if (target && this._barrel) {
            var tp = target.node.getWorldPosition();
            var mp = this.node.getWorldPosition();
            var angle = Math.atan2(tp.y - mp.y, tp.x - mp.x) * (180 / Math.PI);
            this._barrel.angle = angle;
          }
          if (this._cooldown > 0 || !target) return;
          this._shoot(target);
          this._cooldown = 1 / this._stats.fireRate;
        };
        return Tower;
      }(Component), _descriptor = _applyDecoratedDescriptor(_class2.prototype, "projectilePrefab", [_dec2], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/TowerData.ts", ['cc'], function (exports) {
  var cclegacy;
  return {
    setters: [function (module) {
      cclegacy = module.cclegacy;
    }],
    execute: function () {
      exports({
        getStats: getStats,
        sellValue: sellValue,
        upgradeCost: upgradeCost
      });
      var _TOWER_CONFIGS;
      cclegacy._RF.push({}, "199ffNHPiZNo56ExvNljDRx", "TowerData", undefined);
      var TowerType = exports('TowerType', /*#__PURE__*/function (TowerType) {
        TowerType[TowerType["Normal"] = 0] = "Normal";
        TowerType[TowerType["Rapid"] = 1] = "Rapid";
        TowerType[TowerType["Splash"] = 2] = "Splash";
        TowerType[TowerType["Slow"] = 3] = "Slow";
        TowerType[TowerType["Sniper"] = 4] = "Sniper";
        return TowerType;
      }({}));
      var TOWER_CONFIGS = exports('TOWER_CONFIGS', (_TOWER_CONFIGS = {}, _TOWER_CONFIGS[TowerType.Normal] = {
        name: '기본',
        desc: '균형잡힌 타워',
        cost: 50,
        color: [60, 120, 220],
        size: 44,
        damage: 30,
        fireRate: 1.0,
        range: 160,
        splashRadius: 0,
        slowFactor: 0,
        slowDuration: 0,
        upgDamage: 1.20,
        upgFireRate: 1.10,
        upgRange: 1.05,
        upgSpecial: 0
      }, _TOWER_CONFIGS[TowerType.Rapid] = {
        name: '연사',
        desc: '공격력 절반·연사 2배',
        cost: 75,
        color: [0, 200, 220],
        size: 36,
        damage: 15,
        fireRate: 2.0,
        range: 140,
        splashRadius: 0,
        slowFactor: 0,
        slowDuration: 0,
        upgDamage: 1.25,
        upgFireRate: 1.15,
        upgRange: 1.0,
        upgSpecial: 0
      }, _TOWER_CONFIGS[TowerType.Splash] = {
        name: '스플레시',
        desc: '광역 피해',
        cost: 80,
        color: [220, 120, 20],
        size: 48,
        damage: 15,
        fireRate: 0.5,
        range: 130,
        splashRadius: 80,
        slowFactor: 0,
        slowDuration: 0,
        upgDamage: 1.20,
        upgFireRate: 1.0,
        upgRange: 1.0,
        upgSpecial: 1.12
      }, _TOWER_CONFIGS[TowerType.Slow] = {
        name: '둔화',
        desc: '피해 없음·속도↓',
        cost: 60,
        color: [160, 60, 220],
        size: 44,
        damage: 0,
        fireRate: 1.0,
        range: 120,
        splashRadius: 0,
        slowFactor: 0.50,
        slowDuration: 2.0,
        upgDamage: 1.0,
        upgFireRate: 1.0,
        upgRange: 1.0,
        upgSpecial: 0.04
      }, _TOWER_CONFIGS[TowerType.Sniper] = {
        name: '장거리',
        desc: '공격력 2배·사거리 2배',
        cost: 80,
        color: [50, 200, 80],
        size: 40,
        damage: 60,
        fireRate: 0.5,
        range: 320,
        splashRadius: 0,
        slowFactor: 0,
        slowDuration: 0,
        upgDamage: 1.25,
        upgFireRate: 1.10,
        upgRange: 1.0,
        upgSpecial: 0
      }, _TOWER_CONFIGS));
      function getStats(type, level) {
        var c = TOWER_CONFIGS[type];
        var n = Math.max(0, Math.min(9, level - 1));
        return {
          damage: c.damage * Math.pow(c.upgDamage, n),
          fireRate: c.fireRate * Math.pow(c.upgFireRate, n),
          range: c.range * Math.pow(c.upgRange, n),
          splashRadius: c.splashRadius > 0 ? c.splashRadius * Math.pow(c.upgSpecial, n) : 0,
          slowFactor: c.slowFactor > 0 ? Math.min(0.85, c.slowFactor + n * c.upgSpecial) : 0,
          slowDuration: c.slowDuration
        };
      }
      function upgradeCost(type, currentLevel) {
        return Math.floor(TOWER_CONFIGS[type].cost * 0.4 * currentLevel);
      }
      function sellValue(type, level) {
        var total = TOWER_CONFIGS[type].cost;
        for (var l = 1; l < level; l++) total += upgradeCost(type, l);
        return Math.floor(total * 0.5);
      }
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/TowerManager.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './GameManager.ts', './PathFinder.ts', './TowerData.ts', './Tower.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, _createForOfIteratorHelperLoose, cclegacy, _decorator, Prefab, Node, UITransform, Vec3, Graphics, Color, Label, view, instantiate, Component, GameManager, GameState, grid, GRID_W, GRID_H, gridToCanvas, SPAWN_CELL, EXIT_CELL, TILE, isPathPossible, cellKey, canvasToGrid, TOWER_CONFIGS, sellValue, upgradeCost, Tower;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
      _createForOfIteratorHelperLoose = module.createForOfIteratorHelperLoose;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Prefab = module.Prefab;
      Node = module.Node;
      UITransform = module.UITransform;
      Vec3 = module.Vec3;
      Graphics = module.Graphics;
      Color = module.Color;
      Label = module.Label;
      view = module.view;
      instantiate = module.instantiate;
      Component = module.Component;
    }, function (module) {
      GameManager = module.GameManager;
      GameState = module.GameState;
    }, function (module) {
      grid = module.grid;
      GRID_W = module.GRID_W;
      GRID_H = module.GRID_H;
      gridToCanvas = module.gridToCanvas;
      SPAWN_CELL = module.SPAWN_CELL;
      EXIT_CELL = module.EXIT_CELL;
      TILE = module.TILE;
      isPathPossible = module.isPathPossible;
      cellKey = module.cellKey;
      canvasToGrid = module.canvasToGrid;
    }, function (module) {
      TOWER_CONFIGS = module.TOWER_CONFIGS;
      sellValue = module.sellValue;
      upgradeCost = module.upgradeCost;
    }, function (module) {
      Tower = module.Tower;
    }],
    execute: function () {
      var _dec, _dec2, _dec3, _class, _class2, _descriptor, _descriptor2;
      cclegacy._RF.push({}, "660e8HaTUpKX6Jc9bqHxuWY", "TowerManager", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;
      var TowerManager = exports('TowerManager', (_dec = ccclass('TowerManager'), _dec2 = property(Prefab), _dec3 = property(Node), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(TowerManager, _Component);
        function TowerManager() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "towerPrefab", _descriptor, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "towerContainer", _descriptor2, _assertThisInitialized(_this));
          _this._selectedType = null;
          _this._typeButtons = [];
          _this._msgNode = null;
          _this._infoPanel = null;
          _this._towerNodes = new Map();
          // cellKey → tower Node
          _this._pauseBtn = null;
          _this._speedBtn = null;
          return _this;
        }
        var _proto = TowerManager.prototype;
        // ── Lifecycle ─────────────────────────────────────────────────────────────────
        _proto.start = function start() {
          // Clear leftover grid state from previous session
          grid.blocked.clear();
          grid.version++;
          var gm = GameManager.instance;
          if (gm && gm.state === GameState.IDLE) gm.reset();
          this._drawMapBorder();
          this._drawMarkers();
          this._createTypeButtons();
          this._createPauseButton();
          this._createSpeedButton();
          this.node.on(Node.EventType.TOUCH_END, this._onTouch, this);
        }

        // ── Map border & grid lines ───────────────────────────────────────────────────
        ;

        _proto._drawMapBorder = function _drawMapBorder() {
          var n = new Node('MapBorder');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(1280, 720);
          n.setPosition(Vec3.ZERO);
          var g = n.addComponent(Graphics);
          g.strokeColor = new Color(255, 255, 255, 30);
          g.lineWidth = 1;
          for (var c = 0; c <= GRID_W; c++) {
            var x = -640 + c * TILE;
            g.moveTo(x, -360);
            g.lineTo(x, 360);
          }
          for (var r = 0; r <= GRID_H; r++) {
            var y = -360 + r * TILE;
            g.moveTo(-640, y);
            g.lineTo(640, y);
          }
          g.stroke();
          g.strokeColor = new Color(200, 200, 220, 200);
          g.lineWidth = 4;
          g.rect(-640, -360, 1280, 720);
          g.stroke();
          n.setParent(this.node);
        }

        // ── Spawn / Exit markers ──────────────────────────────────────────────────────
        ;

        _proto._drawMarkers = function _drawMarkers() {
          this._makeMarker(gridToCanvas(SPAWN_CELL.col, SPAWN_CELL.row), new Color(60, 200, 60, 220), 'START');
          this._makeMarker(gridToCanvas(EXIT_CELL.col, EXIT_CELL.row), new Color(220, 50, 50, 220), 'EXIT');
        };
        _proto._makeMarker = function _makeMarker(pos, color, label) {
          var n = new Node(label + 'Marker');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(TILE, TILE);
          n.setPosition(new Vec3(pos.x, pos.y, 0));
          var g = n.addComponent(Graphics);
          g.fillColor = color;
          g.circle(0, 0, TILE * 0.4);
          g.fill();
          g.strokeColor = new Color(255, 255, 255, 200);
          g.lineWidth = 3;
          g.circle(0, 0, TILE * 0.4);
          g.stroke();
          var ln = new Node('Lbl');
          ln.layer = this.node.layer;
          ln.addComponent(UITransform).setContentSize(TILE * 1.5, 24);
          ln.setPosition(new Vec3(0, -TILE * 0.55, 0));
          var lbl = ln.addComponent(Label);
          lbl.string = label;
          lbl.fontSize = 16;
          lbl.color = new Color(255, 255, 255, 255);
          ln.setParent(n);
          n.setParent(this.node);
        }

        // ── Tower type buttons ────────────────────────────────────────────────────────
        ;

        _proto._createTypeButtons = function _createTypeButtons() {
          var _this2 = this;
          var btnW = 100;
          var btnH = 70;
          var gap = 8;
          var total = 5 * btnW + 4 * gap;
          var leftEdge = -total / 2;
          var _loop = function _loop() {
            var type = i;
            var cfg = TOWER_CONFIGS[type];
            var cx = leftEdge + i * (btnW + gap) + btnW / 2;
            var btn = new Node("TypeBtn_" + i);
            btn.layer = _this2.node.layer;
            btn.addComponent(UITransform).setContentSize(btnW, btnH);
            btn.setPosition(new Vec3(cx, -310, 0));
            var g = btn.addComponent(Graphics);
            _this2._redrawTypeButton(g, type, false);

            // Label on a CHILD node (Graphics + Label on same node conflict)
            var ln = new Node('BtnLbl');
            ln.layer = _this2.node.layer;
            ln.addComponent(UITransform).setContentSize(btnW - 4, btnH - 4);
            ln.setPosition(Vec3.ZERO);
            var lbl = ln.addComponent(Label);
            lbl.string = cfg.name + "\n" + cfg.cost + "G\n" + cfg.desc;
            lbl.fontSize = 11;
            lbl.lineHeight = 14;
            lbl.color = new Color(255, 255, 255, 255);
            ln.setParent(btn);
            btn.on(Node.EventType.TOUCH_END, function (e) {
              e.propagationStopped = true;
              _this2._selectType(type);
            }, _this2);
            btn.setParent(_this2.node);
            _this2._typeButtons[i] = btn;
          };
          for (var i = 0; i < 5; i++) {
            _loop();
          }
        };
        _proto._selectType = function _selectType(type) {
          if (this._selectedType === type) {
            this._selectedType = null;
          } else {
            this._selectedType = type;
            this._hideInfoPanel();
          }
          this._updateTypeButtonVisuals();
        };
        _proto._updateTypeButtonVisuals = function _updateTypeButtonVisuals() {
          for (var i = 0; i < this._typeButtons.length; i++) {
            var btn = this._typeButtons[i];
            if (!btn || !btn.isValid) continue;
            var g = btn.getComponent(Graphics);
            if (!g) continue;
            this._redrawTypeButton(g, i, this._selectedType === i);
          }
        };
        _proto._redrawTypeButton = function _redrawTypeButton(g, type, selected) {
          var cfg = TOWER_CONFIGS[type];
          var _cfg$color = cfg.color,
            r = _cfg$color[0],
            gb = _cfg$color[1],
            b = _cfg$color[2];
          g.clear();
          if (selected) {
            g.fillColor = new Color(Math.min(255, r + 60), Math.min(255, gb + 60), Math.min(255, b + 60), 220);
            g.roundRect(-50, -35, 100, 70, 8);
            g.fill();
            g.strokeColor = new Color(80, 255, 80, 255);
            g.lineWidth = 4;
            g.roundRect(-50, -35, 100, 70, 8);
            g.stroke();
          } else {
            g.fillColor = new Color(Math.floor(r * 0.3), Math.floor(gb * 0.3), Math.floor(b * 0.3), 200);
            g.roundRect(-50, -35, 100, 70, 8);
            g.fill();
            g.strokeColor = new Color(r, gb, b, 120);
            g.lineWidth = 2;
            g.roundRect(-50, -35, 100, 70, 8);
            g.stroke();
          }
        }

        // ── Notification message ──────────────────────────────────────────────────────
        ;

        _proto._showMsg = function _showMsg(text, color) {
          var _this$_msgNode,
            _this3 = this;
          if (color === void 0) {
            color = new Color(255, 220, 50, 255);
          }
          (_this$_msgNode = this._msgNode) == null || _this$_msgNode.destroy();
          var n = new Node('Msg');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(480, 40);
          n.setPosition(new Vec3(0, -240, 0));
          var lbl = n.addComponent(Label);
          lbl.string = text;
          lbl.fontSize = 20;
          lbl.color = color;
          n.setParent(this.node);
          this._msgNode = n;
          this.scheduleOnce(function () {
            if (n.isValid) n.destroy();
            if (_this3._msgNode === n) _this3._msgNode = null;
          }, 2);
        }

        // ── Info / Sell / Upgrade panel ───────────────────────────────────────────────
        ;

        _proto._showInfoPanel = function _showInfoPanel(towerNode, key) {
          var _towerNode$getCompone,
            _gm$gold,
            _this4 = this;
          this._hideInfoPanel();
          (_towerNode$getCompone = towerNode.getComponent(Tower)) == null || _towerNode$getCompone.showRangeCircle();
          var tower = towerNode.getComponent(Tower);
          if (!tower) return;
          var type = tower.towerType;
          var level = tower.level;
          var cfg = TOWER_CONFIGS[type];
          var sell = sellValue(type, level);
          var canUp = level < 10;

          // Calculate max affordable upgrades
          var gm = GameManager.instance;
          var maxUpLevels = 0;
          var tempLevel = level;
          var tempGold = (_gm$gold = gm == null ? void 0 : gm.gold) != null ? _gm$gold : 0;
          while (tempLevel < 10) {
            var c = upgradeCost(type, tempLevel);
            if (c > tempGold) break;
            tempGold -= c;
            tempLevel++;
            maxUpLevels++;
          }

          // Panel layout
          // Rows: header(20) + [maxUpBtn(36) + upBtn(36)] if canUp + sellBtn(32) + padding
          var btnGap = 8;
          var upGap = canUp ? 36 + btnGap + 36 + btnGap : 0;
          var panelH = 16 + 20 + btnGap + upGap + 32 + 16; // top pad + header + gap + [up rows] + sell + bot pad

          // Smart position: prefer above tower, fall back to below if near top
          var towerPos = towerNode.getPosition();
          var aboveY = towerPos.y + 50 + panelH * 0.5;
          var belowY = towerPos.y - 50 - panelH * 0.5;
          var panelY = aboveY + panelH * 0.5 <= 350 ? aboveY : belowY;
          var panel = new Node('InfoPanel');
          panel.layer = this.node.layer;
          panel.addComponent(UITransform).setContentSize(160, panelH);
          panel.setPosition(new Vec3(towerPos.x, panelY, 0));

          // Background
          var bg = panel.addComponent(Graphics);
          bg.fillColor = new Color(20, 20, 40, 230);
          bg.roundRect(-80, -panelH / 2, 160, panelH, 8);
          bg.fill();
          bg.strokeColor = new Color(150, 150, 200, 200);
          bg.lineWidth = 2;
          bg.roundRect(-80, -panelH / 2, 160, panelH, 8);
          bg.stroke();

          // Layout from top
          var curY = panelH / 2 - 16 - 10; // start below top padding, center of header

          // Header
          this._addLabel(panel, cfg.name + "  Lv." + level, 0, curY, 16, new Color(255, 220, 100, 255));
          curY -= 10 + btnGap; // bottom of header + gap

          if (canUp) {
            var _gm$gold2;
            // Max upgrade button
            curY -= 18; // center of 36px button
            var upCost = upgradeCost(type, level);
            var spentOnMax = ((_gm$gold2 = gm == null ? void 0 : gm.gold) != null ? _gm$gold2 : 0) - tempGold;
            var maxBtn = this._addButton(panel, maxUpLevels > 0 ? "\uCD5C\uB300 \uC5C5\uADF8\uB808\uC774\uB4DC\n\u2192Lv." + tempLevel + "  (" + spentOnMax + "G)" : '골드 부족', 0, curY, 144, 36, maxUpLevels > 0 ? new Color(30, 100, 200, 230) : new Color(80, 80, 80, 180));
            if (maxUpLevels > 0) {
              maxBtn.on(Node.EventType.TOUCH_END, function (e) {
                e.propagationStopped = true;
                _this4._maxUpgradeTower(key);
              }, this);
            }
            curY -= 18 + btnGap; // bottom of maxBtn + gap

            // Single upgrade button
            curY -= 18;
            var upBtn = this._addButton(panel, "\uC5C5\uADF8\uB808\uC774\uB4DC\n" + upCost + "G", 0, curY, 144, 36, new Color(40, 80, 180, 230));
            upBtn.on(Node.EventType.TOUCH_END, function (e) {
              e.propagationStopped = true;
              _this4._upgradeTower(key);
            }, this);
            curY -= 18 + btnGap;
          }

          // Sell button
          curY -= 16; // center of 32px button
          var sellBtn = this._addButton(panel, "\uD310\uB9E4  +" + sell + "G", 0, curY, 144, 32, new Color(160, 40, 40, 220));
          sellBtn.on(Node.EventType.TOUCH_END, function (e) {
            e.propagationStopped = true;
            _this4._sellTower(key);
          }, this);
          panel.on(Node.EventType.TOUCH_END, function (e) {
            e.propagationStopped = true;
          }, this);
          panel.setParent(this.node);
          this._infoPanel = panel;
        }

        /** Label on its own node (no Graphics conflict) */;
        _proto._addLabel = function _addLabel(parent, text, x, y, fs, color) {
          var n = new Node('Lbl');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(152, fs + 6);
          n.setPosition(new Vec3(x, y, 0));
          var lbl = n.addComponent(Label);
          lbl.string = text;
          lbl.fontSize = fs;
          lbl.color = color;
          n.setParent(parent);
          return n;
        }

        /** Button: Graphics bg node + child Label node */;
        _proto._addButton = function _addButton(parent, text, x, y, w, h, color) {
          var n = new Node('Btn');
          n.layer = this.node.layer;
          n.addComponent(UITransform).setContentSize(w, h);
          n.setPosition(new Vec3(x, y, 0));
          var bg = n.addComponent(Graphics);
          bg.fillColor = color;
          bg.roundRect(-w / 2, -h / 2, w, h, 6);
          bg.fill();

          // Label on child node to avoid Graphics/Label render conflict
          var ln = new Node('Lbl');
          ln.layer = this.node.layer;
          ln.addComponent(UITransform).setContentSize(w - 4, h - 4);
          ln.setPosition(Vec3.ZERO);
          var lbl = ln.addComponent(Label);
          lbl.string = text;
          lbl.fontSize = 13;
          lbl.lineHeight = 16;
          lbl.color = new Color(255, 255, 255, 255);
          ln.setParent(n);
          n.setParent(parent);
          return n;
        };
        _proto._hideInfoPanel = function _hideInfoPanel() {
          var _this$_infoPanel;
          // Hide range circle on currently shown tower
          if (this._infoPanel) {
            for (var _iterator = _createForOfIteratorHelperLoose(this._towerNodes), _step; !(_step = _iterator()).done;) {
              var _tNode$getComponent;
              var _step$value = _step.value,
                tNode = _step$value[1];
              if (tNode != null && tNode.isValid) (_tNode$getComponent = tNode.getComponent(Tower)) == null || _tNode$getComponent.hideRangeCircle();
            }
          }
          (_this$_infoPanel = this._infoPanel) == null || _this$_infoPanel.destroy();
          this._infoPanel = null;
        }

        // ── Upgrade ───────────────────────────────────────────────────────────────────
        ;

        _proto._upgradeTower = function _upgradeTower(key) {
          var node = this._towerNodes.get(key);
          if (!(node != null && node.isValid)) {
            this._hideInfoPanel();
            return;
          }
          var tower = node.getComponent(Tower);
          if (!tower) return;
          if (tower.level >= 10) {
            this._showMsg('이미 최대 레벨입니다!');
            this._hideInfoPanel();
            return;
          }
          var cost = upgradeCost(tower.towerType, tower.level);
          var gm = GameManager.instance;
          if (!gm || !gm.spendGold(cost)) {
            this._showMsg("\uACE8\uB4DC \uBD80\uC871! (\uD544\uC694: " + cost + "G)", new Color(255, 80, 80, 255));
            this._hideInfoPanel();
            return;
          }
          tower.upgrade();
          this._hideInfoPanel();
          this._showMsg("\uC5C5\uADF8\uB808\uC774\uB4DC! Lv." + tower.level, new Color(100, 220, 255, 255));
        };
        _proto._maxUpgradeTower = function _maxUpgradeTower(key) {
          var node = this._towerNodes.get(key);
          if (!(node != null && node.isValid)) {
            this._hideInfoPanel();
            return;
          }
          var tower = node.getComponent(Tower);
          if (!tower) return;
          var gm = GameManager.instance;
          if (!gm) return;
          var upgraded = 0;
          while (tower.level < 10) {
            var cost = upgradeCost(tower.towerType, tower.level);
            if (!gm.spendGold(cost)) break;
            tower.upgrade();
            upgraded++;
          }
          this._hideInfoPanel();
          if (upgraded > 0) {
            this._showMsg(upgraded + "\uB808\uBCA8 \uC5C5! \u2192 Lv." + tower.level, new Color(100, 220, 255, 255));
          } else {
            this._showMsg('골드 부족!', new Color(255, 80, 80, 255));
          }
        }

        // ── Sell ──────────────────────────────────────────────────────────────────────
        ;

        _proto._sellTower = function _sellTower(key) {
          var _GameManager$instance;
          var node = this._towerNodes.get(key);
          if (!(node != null && node.isValid)) {
            this._hideInfoPanel();
            return;
          }
          var tower = node.getComponent(Tower);
          var sell = tower ? sellValue(tower.towerType, tower.level) : 25;
          (_GameManager$instance = GameManager.instance) == null || _GameManager$instance.addGold(sell);
          grid.blocked["delete"](key);
          grid.version++;
          node.destroy();
          this._towerNodes["delete"](key);
          this._hideInfoPanel();
          this._showMsg("\uD310\uB9E4! +" + sell + "G", new Color(100, 220, 100, 255));
        }

        // ── Pause button ─────────────────────────────────────────────────────────────
        ;

        _proto._createPauseButton = function _createPauseButton() {
          var _this5 = this;
          var W = 90,
            H = 36;
          var btn = new Node('PauseBtn');
          btn.layer = this.node.layer;
          btn.addComponent(UITransform).setContentSize(W, H);
          btn.setPosition(new Vec3(545, 330, 0));
          btn.addComponent(Graphics); // background only

          var ln = new Node('Lbl');
          ln.layer = this.node.layer;
          ln.addComponent(UITransform).setContentSize(W - 4, H - 4);
          ln.setPosition(Vec3.ZERO);
          ln.addComponent(Label).fontSize = 14;
          ln.getComponent(Label).color = new Color(255, 255, 255, 255);
          ln.setParent(btn);
          this._pauseBtn = btn;
          this._refreshPauseBtn();
          btn.on(Node.EventType.TOUCH_END, function (e) {
            e.propagationStopped = true;
            var gm = GameManager.instance;
            if (!gm) return;
            if (gm.state === GameState.PAUSED) gm.resume();else if (gm.state === GameState.PLAYING) gm.pause();
            _this5._refreshPauseBtn();
          }, this);
          btn.setParent(this.node);
        };
        _proto._refreshPauseBtn = function _refreshPauseBtn() {
          var _this$_pauseBtn, _GameManager$instance2, _this$_pauseBtn$getCh;
          if (!((_this$_pauseBtn = this._pauseBtn) != null && _this$_pauseBtn.isValid)) return;
          var paused = ((_GameManager$instance2 = GameManager.instance) == null ? void 0 : _GameManager$instance2.state) === GameState.PAUSED;
          var W = 90,
            H = 36;
          var g = this._pauseBtn.getComponent(Graphics);
          g.clear();
          g.fillColor = paused ? new Color(50, 150, 50, 220) : new Color(160, 80, 20, 220);
          g.roundRect(-W / 2, -H / 2, W, H, 6);
          g.fill();
          var lbl = (_this$_pauseBtn$getCh = this._pauseBtn.getChildByName('Lbl')) == null ? void 0 : _this$_pauseBtn$getCh.getComponent(Label);
          if (lbl) lbl.string = paused ? '재개' : '일시정지';
        }

        // ── Speed button ──────────────────────────────────────────────────────────────
        ;

        _proto._createSpeedButton = function _createSpeedButton() {
          var _this6 = this;
          var W = 70,
            H = 36;
          var btn = new Node('SpeedBtn');
          btn.layer = this.node.layer;
          btn.addComponent(UITransform).setContentSize(W, H);
          btn.setPosition(new Vec3(460, 330, 0));
          btn.addComponent(Graphics);
          var ln = new Node('Lbl');
          ln.layer = this.node.layer;
          ln.addComponent(UITransform).setContentSize(W - 4, H - 4);
          ln.setPosition(Vec3.ZERO);
          ln.addComponent(Label).fontSize = 14;
          ln.getComponent(Label).color = new Color(255, 255, 255, 255);
          ln.setParent(btn);
          this._speedBtn = btn;
          this._refreshSpeedBtn();
          btn.on(Node.EventType.TOUCH_END, function (e) {
            e.propagationStopped = true;
            var gm = GameManager.instance;
            if (!gm) return;
            gm.setSpeed(gm.speed === 1 ? 2 : 1);
            _this6._refreshSpeedBtn();
          }, this);
          btn.setParent(this.node);
        };
        _proto._refreshSpeedBtn = function _refreshSpeedBtn() {
          var _this$_speedBtn, _GameManager$instance3, _this$_speedBtn$getCh;
          if (!((_this$_speedBtn = this._speedBtn) != null && _this$_speedBtn.isValid)) return;
          var is2x = ((_GameManager$instance3 = GameManager.instance) == null ? void 0 : _GameManager$instance3.speed) === 2;
          var W = 70,
            H = 36;
          var g = this._speedBtn.getComponent(Graphics);
          g.clear();
          g.fillColor = is2x ? new Color(180, 130, 20, 220) : new Color(40, 80, 150, 220);
          g.roundRect(-W / 2, -H / 2, W, H, 6);
          g.fill();
          var lbl = (_this$_speedBtn$getCh = this._speedBtn.getChildByName('Lbl')) == null ? void 0 : _this$_speedBtn$getCh.getComponent(Label);
          if (lbl) lbl.string = is2x ? '>> 2배속' : '> 1배속';
        }

        // ── Map touch ─────────────────────────────────────────────────────────────────
        ;

        _proto._onTouch = function _onTouch(e) {
          var gm = GameManager.instance;
          var ui = e.getUILocation();
          var wx = ui.x - 640;
          var wy = ui.y - view.getFrameSize().height / 2;
          var _canvasToGrid = canvasToGrid(wx, wy),
            col = _canvasToGrid.col,
            row = _canvasToGrid.row;
          var key = cellKey(col, row);

          // Close info panel on any map click
          if (this._infoPanel) {
            this._hideInfoPanel();
            return;
          }

          // Not in build mode: tap tower to show info panel
          if (this._selectedType === null) {
            if (grid.blocked.has(key)) {
              var towerNode = this._towerNodes.get(key);
              if (towerNode) this._showInfoPanel(towerNode, key);
            }
            return;
          }

          // ── Build mode ────────────────────────────────────────────────────────────

          if (!gm || !this.towerPrefab || !this.towerContainer) return;
          var type = this._selectedType;
          var cfg = TOWER_CONFIGS[type];
          if (col === SPAWN_CELL.col && row === SPAWN_CELL.row || col === EXIT_CELL.col && row === EXIT_CELL.row) {
            this._showMsg('여기엔 건설할 수 없습니다!');
            return;
          }
          if (grid.blocked.has(key)) {
            this._showMsg('이미 건설된 타워가 있습니다!');
            return;
          }
          grid.blocked.add(key);
          if (!isPathPossible(grid.blocked)) {
            grid.blocked["delete"](key);
            this._showMsg('경로가 막힙니다! 다른 곳에 건설하세요.', new Color(255, 80, 80, 255));
            return;
          }
          if (!gm.spendGold(cfg.cost)) {
            grid.blocked["delete"](key);
            this._showMsg("\uACE8\uB4DC \uBD80\uC871! (\uD544\uC694: " + cfg.cost + "G, \uBCF4\uC720: " + gm.gold + "G)", new Color(255, 80, 80, 255));
            return;
          }
          grid.version++;
          var tilePos = gridToCanvas(col, row);
          var node = instantiate(this.towerPrefab);
          this.towerContainer.addChild(node);
          node.setPosition(tilePos.x, tilePos.y, 0);
          var tower = node.getComponent(Tower);
          tower == null || tower.init(type);
          this._towerNodes.set(key, node);
          this._selectedType = null;
          this._updateTypeButtonVisuals();
        };
        return TowerManager;
      }(Component), (_descriptor = _applyDecoratedDescriptor(_class2.prototype, "towerPrefab", [_dec2], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _descriptor2 = _applyDecoratedDescriptor(_class2.prototype, "towerContainer", [_dec3], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      })), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/UIManager.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './GameManager.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, cclegacy, _decorator, Label, Component, GameManager;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Label = module.Label;
      Component = module.Component;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _dec2, _dec3, _dec4, _dec5, _class, _class2, _descriptor, _descriptor2, _descriptor3, _descriptor4;
      cclegacy._RF.push({}, "932d97iMFJBp6xCNXs/ZBYS", "UIManager", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;
      var UIManager = exports('UIManager', (_dec = ccclass('UIManager'), _dec2 = property(Label), _dec3 = property(Label), _dec4 = property(Label), _dec5 = property(Label), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(UIManager, _Component);
        function UIManager() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "goldLabel", _descriptor, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "livesLabel", _descriptor2, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "waveLabel", _descriptor3, _assertThisInitialized(_this));
          _initializerDefineProperty(_this, "scoreLabel", _descriptor4, _assertThisInitialized(_this));
          return _this;
        }
        var _proto = UIManager.prototype;
        _proto.update = function update() {
          var gm = GameManager.instance;
          if (!gm) return;
          if (this.goldLabel) this.goldLabel.string = "Gold: " + gm.gold;
          if (this.livesLabel) this.livesLabel.string = "Lives: " + gm.lives;
          if (this.waveLabel) this.waveLabel.string = "Wave: " + gm.wave;
          if (this.scoreLabel) this.scoreLabel.string = "Score: " + gm.score;
        };
        return UIManager;
      }(Component), (_descriptor = _applyDecoratedDescriptor(_class2.prototype, "goldLabel", [_dec2], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _descriptor2 = _applyDecoratedDescriptor(_class2.prototype, "livesLabel", [_dec3], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _descriptor3 = _applyDecoratedDescriptor(_class2.prototype, "waveLabel", [_dec4], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      }), _descriptor4 = _applyDecoratedDescriptor(_class2.prototype, "scoreLabel", [_dec5], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return null;
        }
      })), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

System.register("chunks:///_virtual/WaveManager.ts", ['./rollupPluginModLoBabelHelpers.js', 'cc', './EnemySpawner.ts', './Enemy.ts', './GameManager.ts'], function (exports) {
  var _applyDecoratedDescriptor, _inheritsLoose, _initializerDefineProperty, _assertThisInitialized, cclegacy, _decorator, Component, EnemySpawner, EnemyType, GameManager;
  return {
    setters: [function (module) {
      _applyDecoratedDescriptor = module.applyDecoratedDescriptor;
      _inheritsLoose = module.inheritsLoose;
      _initializerDefineProperty = module.initializerDefineProperty;
      _assertThisInitialized = module.assertThisInitialized;
    }, function (module) {
      cclegacy = module.cclegacy;
      _decorator = module._decorator;
      Component = module.Component;
    }, function (module) {
      EnemySpawner = module.EnemySpawner;
    }, function (module) {
      EnemyType = module.EnemyType;
    }, function (module) {
      GameManager = module.GameManager;
    }],
    execute: function () {
      var _dec, _class, _class2, _descriptor;
      cclegacy._RF.push({}, "7148bDhPHJPnrdq/n6ywvSd", "WaveManager", undefined);
      var ccclass = _decorator.ccclass,
        property = _decorator.property;
      var WaveManager = exports('WaveManager', (_dec = ccclass('WaveManager'), _dec(_class = (_class2 = /*#__PURE__*/function (_Component) {
        _inheritsLoose(WaveManager, _Component);
        function WaveManager() {
          var _this;
          for (var _len = arguments.length, args = new Array(_len), _key = 0; _key < _len; _key++) {
            args[_key] = arguments[_key];
          }
          _this = _Component.call.apply(_Component, [this].concat(args)) || this;
          _initializerDefineProperty(_this, "waveDelay", _descriptor, _assertThisInitialized(_this));
          _this._waves = [{
            count: 5,
            interval: 1.5
          }, {
            count: 8,
            interval: 1.2,
            speedMult: 1.1
          }, {
            count: 12,
            interval: 1.0,
            hpMult: 1.5
          }, {
            count: 15,
            interval: 0.8,
            hpMult: 2.0,
            speedMult: 1.2
          }, {
            count: 20,
            interval: 0.6,
            hpMult: 2.5,
            speedMult: 1.3
          }];
          _this._spawner = null;
          _this._waveIdx = 0;
          _this._spawnTimer = 0;
          _this._spawned = 0;
          _this._running = false;
          _this._bossSpawned = false;
          return _this;
        }
        var _proto = WaveManager.prototype;
        _proto.start = function start() {
          var _this2 = this;
          this._spawner = this.getComponent(EnemySpawner);
          this.scheduleOnce(function () {
            return _this2._startWave();
          }, 2);
        };
        _proto._waveForIdx = function _waveForIdx(idx) {
          if (idx < this._waves.length) return this._waves[idx];
          var extra = idx - this._waves.length + 1;
          return {
            count: Math.min(20 + extra * 3, 60),
            interval: Math.max(0.6 - extra * 0.02, 0.25),
            hpMult: 2.5 + extra * 0.4,
            speedMult: 1.3 + extra * 0.05
          };
        };
        _proto._pickEnemyType = function _pickEnemyType(waveIdx) {
          var r = Math.random();
          if (waveIdx < 2) {
            return EnemyType.Basic;
          } else if (waveIdx < 4) {
            return r < 0.70 ? EnemyType.Basic : EnemyType.Speed;
          } else if (waveIdx < 6) {
            if (r < 0.50) return EnemyType.Basic;
            if (r < 0.80) return EnemyType.Speed;
            return EnemyType.Tank;
          } else {
            var extra = waveIdx - 6;
            var tankChance = Math.min(0.40, 0.15 + extra * 0.03);
            var speedChance = Math.min(0.35, 0.20 + extra * 0.02);
            if (r < tankChance) return EnemyType.Tank;
            if (r < tankChance + speedChance) return EnemyType.Speed;
            return EnemyType.Basic;
          }
        };
        _proto._startWave = function _startWave() {
          var gm = GameManager.instance;
          if (gm) gm.wave = this._waveIdx + 1;
          this._spawned = 0;
          this._spawnTimer = 0;
          this._running = true;
          this._bossSpawned = false;
        };
        _proto.update = function update(dt) {
          var _GameManager$instance,
            _GameManager$instance2,
            _this3 = this;
          var ts = (_GameManager$instance = (_GameManager$instance2 = GameManager.instance) == null ? void 0 : _GameManager$instance2.timeScale) != null ? _GameManager$instance : 1;
          if (ts === 0 || !this._running) return;
          var edt = dt * ts;
          var w = this._waveForIdx(this._waveIdx);
          this._spawnTimer -= edt;
          if (this._spawnTimer <= 0 && this._spawned < w.count) {
            var _this$_spawner, _w$hpMult, _w$speedMult;
            var type = this._pickEnemyType(this._waveIdx);
            (_this$_spawner = this._spawner) == null || _this$_spawner.spawn((_w$hpMult = w.hpMult) != null ? _w$hpMult : 1, (_w$speedMult = w.speedMult) != null ? _w$speedMult : 1, type);
            this._spawned++;
            this._spawnTimer = w.interval;
          }
          if (this._spawned >= w.count) {
            if (!this._bossSpawned && (this._waveIdx + 1) % 10 === 0) {
              this._bossSpawned = true;
              var bossHpMult = 3 * (this._waveIdx + 1) / 10;
              var bossSpeedMult = 1 + this._waveIdx * 0.02;
              this.scheduleOnce(function () {
                var _this3$_spawner;
                (_this3$_spawner = _this3._spawner) == null || _this3$_spawner.spawnBoss(bossHpMult, bossSpeedMult);
              }, 1.0);
            }
            this._running = false;
            this._waveIdx++;
            this.scheduleOnce(function () {
              return _this3._startWave();
            }, this.waveDelay);
          }
        };
        return WaveManager;
      }(Component), _descriptor = _applyDecoratedDescriptor(_class2.prototype, "waveDelay", [property], {
        configurable: true,
        enumerable: true,
        writable: true,
        initializer: function initializer() {
          return 5;
        }
      }), _class2)) || _class));
      cclegacy._RF.pop();
    }
  };
});

(function(r) {
  r('virtual:///prerequisite-imports/main', 'chunks:///_virtual/main'); 
})(function(mid, cid) {
    System.register(mid, [cid], function (_export, _context) {
    return {
        setters: [function(_m) {
            var _exportObj = {};

            for (var _key in _m) {
              if (_key !== "default" && _key !== "__esModule") _exportObj[_key] = _m[_key];
            }
      
            _export(_exportObj);
        }],
        execute: function () { }
    };
    });
});
//# sourceMappingURL=index.js.map