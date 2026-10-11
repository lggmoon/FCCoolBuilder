;--------------------------------------------------------------
; Constants
;--------------------------------------------------------------
SCREEN_WIDTH = $20     ; 屏幕宽度32
SCREEN_HIGHT = $1E     ; 屏幕宽度30
FLUSH_FLAG_WRITE = $1
FLUSH_FLAG_CLEAR = $ff

;--------------------------------------------------------------
; 等待VBlank
;--------------------------------------------------------------
func_wait_for_vblank:
  BIT PPU_STATUS
  BPL func_wait_for_vblank
  rts

;--------------------------------------------------------------
; start ppu
;--------------------------------------------------------------
func_start_ppu:
  lda #%00000000   ; 启用NMI, 背景使用Pattern Table 0 精灵和北京都是在$0000
  sta PPU_CTRL
  lda #%00011110   ; 启用精灵和背景渲染
  sta PPU_MASK
  rts

func_pause_ppu:
  lda #%00000000   ; 禁用精灵和背景渲染
  sta PPU_MASK
  rts

func_restore_ppu:
  ; 重置PPU滚动
  lda #$00
  sta PPU_SCROLL
  sta PPU_SCROLL
  lda #%00011110   ; 启用精灵和背景渲染
  sta PPU_MASK
  rts


;--------------------------------------------------------------
; clear ppu
;--------------------------------------------------------------
func_clear_ppu_vram:
  ;清理调色板
  jsr local_func_clear_ppu_palette
  ;清理命名表
  jsr local_func_clear_ppu_nametable
  rts

;--------------------------------------------------------------
; 清空 PPU 调色板(PPU 调色板内存: $3F00 - $3F1F)
; 重要: 只能在 Vblank 期间操作调色板, 否则即使屏幕关闭, 也会出现撕裂的色条横线
;--------------------------------------------------------------
local_func_clear_ppu_palette:
  lda #$3F
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$00
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位
  lda #$0F
  ldx #$20
.loop:
  sta PPU_DATA        ;$2007
  dex
  bne .loop
  rts

;--------------------------------------------------------------
;清空 PPU 命名表(PPU 命名表内存: $2000 - $2FFF)
;--------------------------------------------------------------
local_func_clear_ppu_nametable:
  lda #$20
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$00
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位
  lda #$00
  ldx #$10
  ldy #$00
.loop:
  sta PPU_DATA        ;$2007
  iny
  bne .loop
  dex
  bne .loop
  rts

;--------------------------------------------------------------
; 初始化PPU调色板数据与调色板缓冲
; 输入参数 ppu_palette_data来自 build里0.col文件
;--------------------------------------------------------------
func_init_ppu_palette:
  lda #$3F
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$00
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位
  ldx #$00
  ldy #$20
.loop:
  lda ppu_palette_data, X
  sta PPU_DATA        ;$2007      ;写入 PPU 数据
  inx
  dey
  bne .loop
  rts

;--------------------------------------------------------------
; 初始化 PPU 命名表(PPU 命名表内存: $2000 - $2FFF)
; 输入参数 background_nametable_data来自 build里0.nam文件
;--------------------------------------------------------------
func_init_ppu_nametable:
  lda #LOW(background_nametable_data)
  sta v_data_addr
  lda #HIGH(background_nametable_data)
  sta v_data_addr + 1

  lda #$20
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$00
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位

  ldx #4
  ldy #0
.loopy:
  lda [v_data_addr], y
  sta PPU_DATA        ;$2007
  iny
  bne .loopy
  clc
  lda v_data_addr + 1
  adc #$1
  sta v_data_addr + 1
  dex
  bne .loopy
.end
  rts

;--------------------------------------------------------------
; 初始化 PPU 命名表(PPU 命名表内存: $23c0 - $23FF
; 输入参数 background_nametable_data来自 build里0.nam文件
;--------------------------------------------------------------
func_init_ppu_attrtable:
  lda #$23
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$C0
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位

  ldy #$00
.loop:
  lda background_attrtable_data, y
  sta v_attr_buffer, y
  sta PPU_DATA      ;$2007
  iny
  cpy #$40          ;长度
  bne .loop
  rts

func_reload_ppu_attrtable_buffer:
  ldy #$00
.loop:
  lda background_attrtable_data, y
  sta v_attr_buffer, y
  iny
  cpy #$40          ;长度
  bne .loop
  rts


;-------------------------------------------
; 向nametable0写入文本数据
; param v_ppu_addr v_screen_point
;-------------------------------------------
func_draw_text_to_nametable0:
  lda #$20
  sta v_ppu_addr + 1
  lda #$00
  sta v_ppu_addr
  jsr local_func_ppu_addr_offset_screen_point

  ;添加一条缓存
  ldy v_ppu_buffer_count
  lda #FLUSH_FLAG_WRITE
  sta v_ppu_buffer,y
  iny
  lda v_data_addr + 1
  sta v_ppu_buffer,y
  iny
  lda v_data_addr
  sta v_ppu_buffer,y
  iny
  lda v_data_bank
  sta v_ppu_buffer,y
  iny
  lda v_ppu_addr + 1
  sta v_ppu_buffer,y
  iny
  lda v_ppu_addr
  sta v_ppu_buffer,y
  iny
  sty v_ppu_buffer_count

  ;填充数据到 vtemp的内存
  ldx v_screen_point_x
  ldy #$00
.while
  cpx #SCREEN_WIDTH
  bpl .draw_end
.draw_loop:
  lda [v_data_addr], y
  beq .draw_end
  iny
  inx
  jmp .while
.draw_end:
  tya
  ldy v_ppu_buffer_count
  sta v_ppu_buffer,Y
  iny
  sty v_ppu_buffer_count
  rts

;-------------------------------------------
; 向v_ppu_addr地址写入文本数据
; param v_ppu_addr v_screen_point
;-------------------------------------------
local_func_draw_text_to_ppu_addr:
  lda v_ppu_addr + 1
  sta PPU_ADDR
  lda v_ppu_addr
  sta PPU_ADDR

  ldx v_screen_point_x
  ldy #$00
.while
  cpx #SCREEN_WIDTH
  bpl .draw_end
.draw_loop:
  lda [v_data_addr], y
  beq .draw_end
  sta PPU_DATA
  iny
  inx
  jmp .while
.draw_end:
  rts

;-------------------------------------------
; 向v_ppu_addr地址写入大文本第二行数据
; param v_ppu_addr v_screen_point
;-------------------------------------------
local_func_draw_text2line_to_ppu_addr:
  lda v_ppu_addr + 1
  sta PPU_ADDR
  lda v_ppu_addr
  sta PPU_ADDR

  ldx v_screen_point_x
  ldy #$00
.while
  cpx #SCREEN_WIDTH
  bpl .draw_end
.draw_loop:
  lda [v_data_addr], y
  beq .draw_end
  clc
  adc #16
  sta PPU_DATA
  iny
  inx
  jmp .while
.draw_end:
  rts

;-------------------------------------------
; 以v_ppu_addr为页面起始, 根据v_screen_point更新v_ppu_addr
; param v_ppu_addr, v_screen_point
; update v_ppu_addr
;-------------------------------------------
local_func_ppu_addr_offset_screen_point:
  ldx v_screen_point_x
  ldy v_screen_point_y
  ; 如果Y=0，直接跳转到X坐标处理
  cpy #$00
  beq .add_x_coord
.multiply_loop:
  ; 将SCREEN_WIDTH加到地址中
  clc
  lda v_ppu_addr
  adc #SCREEN_WIDTH
  sta v_ppu_addr
  lda v_ppu_addr + 1
  adc #$00           ; 处理进位
  sta v_ppu_addr + 1
  ; 递减Y计数器
  dey
  bne .multiply_loop  ; 继续循环直到Y=0
.add_x_coord:
  ; 加上X坐标
  clc
  txa                   ; 将X坐标加载到累加器
  adc v_ppu_addr       ; 加上地址低字节
  sta v_ppu_addr       ; 存储结果低字节
  lda v_ppu_addr + 1
  adc #$00              ; 处理进位
  sta v_ppu_addr + 1   ; 存储结果高字节
  rts

;------------------------------------------
; 刷新ppu缓存
;-------------------------------------------
func_flush_ppu_buffer
  ldy v_ppu_buffer_count
  beq .end
  LDA #$00
  STA v_ppu_buffer_count
  STA v_ppu_buffer,y
  tay
  jsr local_flush_ppu_buffer_select
.end
  rts

local_flush_ppu_buffer_select
  lda v_ppu_buffer,y
  iny
  cmp #FLUSH_FLAG_WRITE
  beq local_flush_ppu_buffer_write
  cmp #FLUSH_FLAG_CLEAR
  beq local_flush_ppu_buffer_clear
  rts

local_flush_ppu_buffer_write
  LDA v_ppu_buffer,y
  sta v_data_addr + 1
  iny
  LDA v_ppu_buffer,y
  sta v_data_addr
  iny
  LDA v_ppu_buffer,y
  jsr select_prg_bank
  iny
  LDA v_ppu_buffer,y
  STA PPU_ADDR
  .ifdef ENABLE_BIG_FONT
    STA v_temp +1         ;$2006
  .endif
  iny
  LDA v_ppu_buffer,y
  STA PPU_ADDR
  .ifdef ENABLE_BIG_FONT
    STA v_temp        ;$2006
  .endif
  iny
  LDX v_ppu_buffer,y
  iny
  tya
  pha
  ldy #0
.loop_write_data
  LDA [v_data_addr],y
  STA PPU_DATA            ;$2007
  iny
  dex
  BNE .loop_write_data

  ;画大字符第二行
  .ifdef ENABLE_BIG_FONT
    ;计算新地址
    clc
    lda v_temp
    adc #SCREEN_WIDTH
    sta v_temp
    lda v_temp + 1
    adc #0
    sta PPU_ADDR
    lda v_temp
    sta PPU_ADDR

    pla
    tay
    dey
    LDX v_ppu_buffer,y
    iny
    tya
    pha
    ldy #0
.loop_write_data2
    LDA [v_data_addr],y
    clc
    adc #16
    STA PPU_DATA            ;$2007
    iny
    dex
    BNE .loop_write_data2
  .endif

  pla
  tay
  jmp local_flush_ppu_buffer_select

local_flush_ppu_buffer_clear
  LDA v_ppu_buffer,y
  STA PPU_ADDR         ;$2006
  INy
  LDA v_ppu_buffer,y
  STA PPU_ADDR         ;$2006
  INy
  LDx v_ppu_buffer,y
  INy
  lda #0
.loop_clear
  STA PPU_DATA            ;$2007
  DEY
  BNE .loop_clear
  jmp local_flush_ppu_buffer_select

;------------------------------------------
; 拷贝数据到nametabke0的v_screen_offset位置
; param : v_screen_offset v_data_addr v_data_len
;-------------------------------------------
func_copy_data_to_nametable0:
  clc
  lda v_screen_offset
  adc #$00
  sta v_ppu_addr
  lda v_ppu_addr + 1
  adc #$20
  sta v_ppu_addr + 1

  jsr local_func_copy_data_to_nametable
  rts

;------------------------------------------
; 拷贝数据到 v_ppu_addr
; param : v_ppu_addr v_data_addr v_data_len
local_func_copy_data_to_nametable:
  lda v_ppu_addr + 1
  sta PPU_ADDR
  lda v_ppu_addr
  sta PPU_ADDR

  ;保存v_data_addr 高地址
  lda v_data_addr + 1
  pha

  stx v_data_len + 1
.while_high:
  cpx #$00
  beq .draw_low
  ldy #$00
.high_loop:
  lda [v_data_addr], Y
  sta PPU_DATA
  iny
  bne .high_loop
  dex

  ;更新 v_data_addr + 1
  clc
  lda v_data_addr + 1
  adc #$1
  sta v_data_addr + 1
  jmp .while_high
.draw_low:
  ldy #$00
.draw_low_loop:
  lda [v_data_addr], Y
  sta PPU_DATA
  iny
  cpy v_data_len
  bne .draw_low_loop
.draw_end
  ;恢复v_data_addr
  pla
  sta v_data_addr + 1
  rts

;-------------------------------------------
; 向attribute设置v_data_addr的文本高亮
; param v_data_addr v_ppu_addr v_screen_point v_attr_value
;-------------------------------------------
func_set_text_highlight_to_attr_buffer:
  lda v_screen_point_x
  pha

  ldx v_screen_point_x
  ldy #$00
.while
  cpx #SCREEN_WIDTH
  bpl .draw_end
.draw_loop:
  lda [v_data_addr], y
  beq .draw_end
  stx v_screen_point_x
  lda v_attr_value
  pha
  jsr local_func_set_value_at_screen_point
  pla
  sta v_attr_value
  iny
  inx
  jmp .while
.draw_end:
  pla
  sta v_screen_point_x
  lda #1
  sta v_attr_buffer_needupdate
  rts

;-------------------------------------------
; 向attr0写入文本数据
; param v_ppu_addr, v_screen_point, v_attr_value
;-------------------------------------------
local_func_set_value_at_screen_point:
  pha
  txa
  pha
  tya
  pha

  lda v_screen_point_y
  lsr A
  lsr A
  tay ;y里保存attr的y
  ;c标志是y的奇偶，attr的字节里 0 1 2 3，这里根据奇偶计算是 01 还是 23，然后入栈
  lda #0
  adc #0
  asl A
  pha ;压栈y方向奇偶

  ;循环y=y*8
  lda #0
  cpy #0
  beq .clc_x
.loop_y
  clc
  adc #8
  dey
  bne .loop_y
  tay
.clc_x
  sty v_temp

  ;计算attr的x
  lda v_screen_point_x
  lsr A
  lsr A
  tax ;x里保存attr的x
  ;c标志是x的奇偶，attr的字节里 0 1 2 3，这里根据奇偶计算tilepos是 0 1 2 3？然后入栈
  pla ;拉出y方向奇偶
  adc #0
  asl A ;与x方向的奇偶合并
  pha  ;压栈xy奇偶

  ;计算最终偏移y*8(v_temp)+x
  txa
  clc
  adc v_temp
  sta v_temp ;

  ;根据栈的tilepos 循环位移
  pla
  pha
  tax
  lda v_attr_value ;attr里设置1就达到变色的效果了
  cpx #0
  beq .end1
.loop_shift1
  asl A ;attr_value 左移x表示设置x块
  dex
  bne .loop_shift1
.end1
  sta v_attr_value

  ;计算mask
  pla
  tax
  lda #3
  cpx #0
  beq .end2
.loop_shift2
  asl A ; 左移x表示设置x块
  dex
  bne .loop_shift2
.end2
  eor #$FF ;疑惑得到取反的mask
  ldx v_temp ;加载偏移
  and v_attr_buffer,x ; mask原数据
  ora v_attr_value ; 与新数据or
  sta v_attr_buffer,x ; 存到新数据

  pla
  tay
  pla
  tax
  pla
  rts

;----------------------------------------------
; 适配
;----------------------------------------------
func_load_base_chr:
  ; loading CHR
  lda #BANK(chr_data) / 2 ; bank with CHR
  jsr select_prg_bank
  lda #LOW(chr_data)
  sta COPY_SOURCE_ADDR
  lda #HIGH(chr_data)
  sta COPY_SOURCE_ADDR+1
  jsr load_chr
  rts

  ; nametable cleanup
clear_screen:
  lda #$20
  sta PPUADDR
  lda #$00
  sta PPUADDR
  lda #$00
  ldx #0
  ldy #$10
.loop:
  sta PPUDATA
  inx
  bne .loop
  dey
  bne .loop
  rts

; clear all sprites data
clear_sprites:
  lda #$FF
  ldx #0
.loop:
  sta SPRITES_DATA, x
  inx
  bne .loop
  rts

  ; DMA sprites loading
sprite_dma_copy:
  pha
  lda #0
  sta OAMADDR
  lda #HIGH(SPRITES_DATA)
  sta OAMDMA
  pla
  rts

waitblank_simple:
  pha
  bit PPU_STATUS
.loop:
  lda PPU_STATUS  ; load A with value at location PPUSTATUS
  bpl .loop  ; if bit 7 is not set (not VBlank) keep checking
  pla
  rts

console_detect:
  jsr waitblank_simple
  ldx #0
  ldy #0
  ; Console type detect
.detect_l:
  inx
  bne .detect_s
  iny
.detect_s:
  lda PPU_STATUS
  bpl .detect_l
  lda #$00
  cpy #$08
  bne .not_ntsc1
  ora #$01
.not_ntsc1:
  cpy #$09
  bne .not_ntsc2
  ora #$01
.not_ntsc2:
  cpy #$0A
  bne .not_pal
  ora #$02
.not_pal:
  cpy #$0B
  bne .not_dendy
  ora #$04
.not_dendy:
  ldx $5000
  beq .console_detect_not_new_dendy
  ora #$08
.console_detect_not_new_dendy:
  sta <CONSOLE_TYPE
  rts