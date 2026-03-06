// yyyymmdd 형식의 문자열 리턴
Date.prototype.yyyymmdd = function() {
     return [this.getFullYear(),
             (this.getMonth() + 1).zf(2),
             this.getDate().zf(2)
           ].join('');
  };
   
  // yyyy-mm-dd 형식의 문자열 리턴
  Date.prototype.yyyymmdd_dash = function() {
    return [this.getFullYear(), '-',
            (this.getMonth() + 1).zf(2) , '-',
            this.getDate().zf(2) + dd
           ].join('');
  };
   
  // yyyy년 mm월 dd일 형식의 문자열 리턴
  Date.prototype.yyyymmdd_kor = function() {
    return [this.getFullYear(), '년 ',
            (this.getMonth() + 1).zf(2), '월 ',
            this.getDate().zf(2), '일'
           ].join('');
  };
   
  Date.prototype.hhmmss = function() {
    return [this.getHours().zf(2), ':',
            this.getMinutes().zf(2), ':',
            this.getSeconds().zf(2)
           ].join('');
  };

  Date.prototype.apm_hhmmss = function() {
    var h = this.getHours() % 12;
    h = h ? h : 12;

    return [this.getHours() < 12 ? "오전 " : "오후 ",
            h.zf(2), ':',
            this.getMinutes().zf(2), ':',
            this.getSeconds().zf(2)
           ].join('');
  };

  String.prototype.string = function(len){var s = '', i = 0; while (i++ < len) { s += this; } return s;};
  String.prototype.zf = function(len){return "0".string(len - this.length) + this;};
  Number.prototype.zf = function(len){return this.toString().zf(len);};

