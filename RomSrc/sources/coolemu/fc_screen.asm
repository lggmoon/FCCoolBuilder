;--------------------------------------------------------------
; Constants
;--------------------------------------------------------------
PAGE_COUNT = (GAMES_COUNT+PAGE_MENU_NUM-1)/PAGE_MENU_NUM ;菜单页数
PAGE_LAST_FIRST_INDEX = (PAGE_COUNT-1)*PAGE_MENU_NUM ;菜单最后一页的起始索引
PAGE_LAST_ITEM_COUNT = GAMES_COUNT - (PAGE_COUNT-1)*PAGE_MENU_NUM ;菜单页数

MENU_CURSOR_X = (MENU_START_X-CURSOR_COLUMN_COUNT)*8+CURSOR_OFFSET_X
MENU_CURSOR_Y = MENU_START_Y*8-1+CURSOR_OFFSET_Y
;--------------------------------------------------------------
; 初始化屏幕
;--------------------------------------------------------------
func_init_screen:
  ; 初始化菜单变量
  lda SELECTED_GAME + 1
  sta v_menu_cur + 1
  lda SELECTED_GAME
  sta v_menu_cur

  jsr func_count_page_cur_to_temp_page_cur
  lda v_temp_page_cur + 1
  sta v_page_cur + 1
  lda v_temp_page_cur
  sta v_page_cur

  lda #BANK(chr_data) / 2 ; bank with CHR
  jsr select_prg_bank
  ; 初始化调色版
  jsr func_init_ppu_palette
  ; 初始化属性表
  jsr func_init_ppu_attrtable
  ; 初始化命名表
  jsr func_init_ppu_nametable
  ; 画菜单
  jsr func_draw_menu_title
  jsr func_draw_menu_page
  jsr func_draw_menu_foot

  ; 初始化精灵
  jsr func_init_cursor

  ; 是否要更新 ppu_buffer
  lda v_ppu_buffer_count
  beq .end
  jsr func_flush_ppu_buffer

  ;是否需要更新attr
  lda v_attr_buffer_needupdate
  bne func_process_attr_buff
.end
  rts

func_update_screen:
  lda #HIGHLIGHT_CODE
  beq .nohighlight
  lda #BANK(chr_data) / 2 ; bank with CHR
  jsr select_prg_bank
  ; 初始化属性表
  jsr func_reload_ppu_attrtable_buffer
  lda #1
  sta v_attr_buffer_needupdate
.nohighlight
  ; 画菜单
  jsr func_draw_menu_title
  jsr func_draw_menu_page
  jsr func_draw_menu_foot
  ; 更新指针位置
  jsr func_update_cursor_y
  rts

func_update_cursor:
  .if HIGHLIGHT_CODE > 0
    ;去高亮上次的cur
    lda v_menu_cur_save +1
    sta v_draw_menu_cur +1
    lda v_menu_cur_save
    sta v_draw_menu_cur
    lda #0
    sta v_attr_value
    jsr func_highlight_menu_item

    ;高亮现在的cure
    lda v_menu_cur +1
    sta v_draw_menu_cur +1
    lda v_menu_cur
    sta v_draw_menu_cur
    lda #HIGHLIGHT_CODE
    sta v_attr_value
    jsr func_highlight_menu_item
  .endif
  ; 更新指针位置
  jsr func_update_cursor_y
  rts

func_process_attr_buff:
  lda #$23
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址高位
  lda #$C0
  sta PPU_ADDR     ;$2006      ;写入 PPU 地址低位
  ldy #$00
.loop:
  lda v_attr_buffer,y
  sta PPU_DATA      ;$2007
  iny
  cpy #$40          ;长度
  bne .loop
  lda #0
  sta v_attr_buffer_needupdate
  rts
;--------------------------------------------------------------
; 初始化精灵
;--------------------------------------------------------------
func_init_cursor:
  lda #0
  sta v_cursor_time
  sta v_cursor_frame

  lda #CURSOR_CODE
  sta v_cursor_code

  lda #%0000000
  sta v_cursor_bling

  lda #MENU_CURSOR_X
  sta v_cursor_x

  jsr func_update_cursor
  rts

func_update_cursor_y:
  sec
  lda v_menu_cur
  sbc v_draw_menu_first
  sta v_temp
  lda v_menu_cur + 1
  sbc v_draw_menu_first + 1
  sta v_temp + 1

  ldy v_temp
  beq .add_inter_end
  clc
  lda #0
.add_inter:
  adc #MENU_INTER_SPACE*8
  dey
  bne .add_inter
.add_inter_end
  clc
  adc #MENU_CURSOR_Y
  sta v_cursor_y
  rts

func_refresh_cursor:
  ; 动画帧更新
  .if (CURSOR_BLING_COUNT > 1) | (CURSOR_FRAME_COUNT > 1)
    clc
    lda v_cursor_time
    adc #1
    sta v_cursor_time
    cmp #CURSOR_REFRESH_TIME
    bne .cursor_end
    lda #0
    sta v_cursor_time

    ; 动画颜色更新
    .if CURSOR_BLING_COUNT > 1
      ; 计算精灵颜色
      clc
      lda v_cursor_bling
      adc #1
      cmp #CURSOR_BLING_COUNT
      bne .set_bling
      lda #0
.set_bling
      sta v_cursor_bling
    .endif

    ; 动画精灵更新
    .if CURSOR_FRAME_COUNT > 1
      ; 滚动精灵帧
      lda v_cursor_frame
      clc
      adc #1
      cmp #CURSOR_FRAME_COUNT
      bne .set_frame
      lda #0
.set_frame
      sta v_cursor_frame

      ; 计算图案位置
      clc
      lda #CURSOR_CODE
      ldy v_cursor_frame
      beq .col_add_end
.col_add
      adc #CURSOR_COLUMN_COUNT
      dey
      bne .col_add
.col_add_end
      sta v_cursor_code
    .endif

.cursor_end
  .endif
  jsr func_write_cursor
  rts

func_write_cursor:
  lda v_cursor_y
  pha
  lda v_cursor_code
  pha
  lda v_cursor_bling
  pha
  lda v_cursor_x
  pha

  lda #$10
  sta OAM_ADDR

  ldy #CURSOR_COLUMN_COUNT
.write_sprite
  lda v_cursor_y  ; 精灵y
  sta OAM_DATA
  lda v_cursor_code ; 精灵代码
  sta OAM_DATA
  lda v_cursor_bling ; 精灵颜色
  sta OAM_DATA
  lda v_cursor_x
  sta OAM_DATA
  .ifdef ENABLE_BIG_CURSOR
    clc
    lda v_cursor_y  ; 精灵y
    adc #8
    sta OAM_DATA
    lda v_cursor_code
    adc #16  ; 精灵代码
    sta OAM_DATA
    lda v_cursor_bling
    sta OAM_DATA
    lda v_cursor_x
    sta OAM_DATA
  .endif
  dey
  beq .write_sprite_end
  clc
  lda v_cursor_code
  adc #1
  sta v_cursor_code
  clc
  lda v_cursor_x
  adc #8
  sta v_cursor_x
  jmp .write_sprite
.write_sprite_end

  pla
  sta v_cursor_x
  pla
  sta v_cursor_bling
  pla
  sta v_cursor_code
  pla
  sta v_cursor_y
  rts

;--------------------------------------------------------------
; 画菜标题
;
;--------------------------------------------------------------
func_draw_menu_title:
  lda #BANK(menu_title) / 2 ; bank with title
  sta v_data_bank
  jsr select_prg_bank

  ; 画标题
  lda #MENU_TITLE_Y
  sta v_screen_point_y
  lda #MENU_TITLE_X
  sta v_screen_point_x

  lda #LOW(menu_title)
  sta v_data_addr
  lda #HIGH(menu_title)
  sta v_data_addr + 1

  jsr func_draw_text_to_nametable0
  rts

func_draw_menu_foot:
  lda #BANK(menu_foot) / 2 ; bank with title
  sta v_data_bank
  jsr select_prg_bank

  ; 画标题
  lda #28
  sta v_screen_point_y
  lda #11
  sta v_screen_point_x

  lda #LOW(menu_foot)
  sta v_data_addr
  lda #HIGH(menu_foot)
  sta v_data_addr + 1

  jsr func_draw_text_to_nametable0
  rts

;--------------------------------------------------------------
; 画菜单页
;--------------------------------------------------------------
func_draw_menu_page:
  ;计算菜单 v_draw_menu_first v_draw_menu_last
  jsr local_func_cacl_page_menu_range
  lda v_draw_menu_first + 1
  sta v_draw_menu_cur + 1
  lda v_draw_menu_first
  sta v_draw_menu_cur

  ;设置起始位置
  lda #MENU_START_Y
  sta v_screen_point_y
  lda #MENU_START_X
  sta v_screen_point_x
.while: ;判断是否到GAMES_COUNT
  ldx v_draw_menu_cur + 1
  cpx v_draw_menu_last + 1
  BCC .do
  bne .end_while
  ldx v_draw_menu_cur
  cpx v_draw_menu_last
  BCC .do
  jmp .end_while
.do
  jsr local_func_draw_menu_item
  clc
  lda v_screen_point_y
  adc #MENU_INTER_SPACE
  sta v_screen_point_y
  ;计数加一
  clc
  lda v_draw_menu_cur
  adc #$01
  sta v_draw_menu_cur
  lda v_draw_menu_cur + 1
  adc #$00
  sta v_draw_menu_cur + 1
  jmp .while
.end_while
  rts

;--------------------------------------------------------------
;获取当前菜单的数据地址并画出
;--------------------------------------------------------------
local_func_draw_menu_item:
  lda v_draw_menu_cur + 1
  jsr select_prg_bank
  clc
  lda #LOW(game_names)
  adc v_draw_menu_cur
  sta v_temp
  lda #HIGH(game_names)
  adc v_draw_menu_cur + 1
  sta v_temp + 1

  clc
  lda v_temp
  adc v_draw_menu_cur
  sta v_temp
  lda v_temp + 1
  adc v_draw_menu_cur + 1
  sta v_temp + 1

  sec
  sbc v_draw_menu_cur + 1
  sbc v_draw_menu_cur + 1
  sta v_temp + 1

  ;选择菜单名字bank
  lda v_draw_menu_cur + 1
  sta v_data_bank
  jsr select_prg_bank


  ldy #0
  lda [v_temp], y
  sta v_data_addr
  iny
  lda [v_temp], y
  sta v_data_addr + 1

  jsr func_draw_text_to_nametable0

  lda #HIGHLIGHT_CODE
  beq .end
  lda v_draw_menu_cur + 1
  cmp v_menu_cur + 1
  bne .end
  lda v_draw_menu_cur
  cmp v_menu_cur
  bne .end

  lda #HIGHLIGHT_CODE
  sta v_attr_value
  jsr func_set_text_highlight_to_attr_buffer
.end
  rts

;--------------------------------------------------------------
;高亮当前菜单
;param v_draw_menu_cur v_attr_value
;--------------------------------------------------------------
func_highlight_menu_item:
  lda #MENU_START_Y
  sta v_screen_point_y
  lda #MENU_START_X
  sta v_screen_point_x

  sec
  lda v_draw_menu_cur
  sbc v_draw_menu_first
  sta v_temp
  lda v_draw_menu_cur + 1
  sbc v_draw_menu_first + 1
  sta v_temp + 1

  ldy v_temp
  beq .add_inter_end
  lda v_screen_point_y
.add_inter:
  clc
  adc #MENU_INTER_SPACE
  dey
  bne .add_inter
  sta v_screen_point_y
.add_inter_end

  clc
  lda #LOW(game_names)
  adc v_draw_menu_cur
  sta v_temp
  lda #HIGH(game_names)
  adc v_draw_menu_cur + 1
  sta v_temp + 1

  clc
  lda v_temp
  adc v_draw_menu_cur
  sta v_temp
  lda v_temp + 1
  adc v_draw_menu_cur + 1
  sta v_temp + 1

  sec
  sbc v_draw_menu_cur + 1
  sbc v_draw_menu_cur + 1
  sta v_temp + 1

  ;选择菜单名字bank
  lda v_draw_menu_cur + 1
  jsr select_prg_bank

  ldy #0
  lda [v_temp], y
  sta v_data_addr
  iny
  lda [v_temp], y
  sta v_data_addr + 1

  jsr func_set_text_highlight_to_attr_buffer

.end
  rts

;--------------------------------------------------------------
;计算第一项菜单索引 设置v_draw_menu_first v_draw_menu_last
;--------------------------------------------------------------
local_func_cacl_page_menu_range:
  lda #$0
  sta v_draw_menu_first + 1
  sta v_draw_menu_first
  ldx v_page_cur + 1
.while_high:
  cpx #$0
  beq .add_low
  ldy #$0
.loop_high:
  clc
  lda v_draw_menu_first
  adc #LOW(PAGE_MENU_NUM)
  sta v_draw_menu_first
  lda v_draw_menu_first + 1
  adc #HIGH(PAGE_MENU_NUM)
  sta v_draw_menu_first + 1
  dey
  bne .loop_high
  dex
  jmp .while_high
.add_low
  ldy v_page_cur
  cpy #$0
  beq .count_last
.loop_low
  clc
  lda v_draw_menu_first
  adc #LOW(PAGE_MENU_NUM)
  sta v_draw_menu_first
  lda v_draw_menu_first + 1
  adc #HIGH(PAGE_MENU_NUM)
  sta v_draw_menu_first + 1
  dey
  bne .loop_low
.count_last:
  clc
  lda v_draw_menu_first
  adc #LOW(PAGE_MENU_NUM)
  sta v_draw_menu_last
  lda v_draw_menu_first + 1
  adc #HIGH(PAGE_MENU_NUM)
  sta v_draw_menu_last + 1
  cmp #HIGH(GAMES_COUNT)
  BCC .out_end
  bne .overflow
  lda v_draw_menu_last
  cmp #LOW(GAMES_COUNT)
  bcc .out_end
.overflow:
  lda #HIGH(GAMES_COUNT)
  sta v_draw_menu_last + 1
  lda #LOW(GAMES_COUNT)
  sta v_draw_menu_last
.out_end:
  rts


func_count_page_cur_to_temp_page_cur:
  lda #0
  sta v_temp_page_cur
  sta v_temp_page_cur + 1
  sta v_temp_menu_cur
  sta v_temp_menu_cur + 1
.loop
  clc
  lda v_temp_menu_cur
  adc #LOW(PAGE_MENU_NUM)
  sta v_temp_menu_cur
  lda v_temp_menu_cur + 1
  adc #HIGH(PAGE_MENU_NUM)
  sta v_temp_menu_cur + 1
  cmp v_menu_cur + 1
  bcc .pre_loop
  bne .end_loop
  lda v_temp_menu_cur
  cmp v_menu_cur
  bcc .pre_loop
  bne .end_loop
.pre_loop
  clc
  lda v_temp_page_cur
  adc #$1
  sta v_temp_page_cur
  lda v_temp_page_cur + 1
  adc #$0
  sta v_temp_page_cur + 1
  jmp .loop
.end_loop
  lda v_temp_page_cur + 1
  cmp #HIGH(PAGE_COUNT)
  bcc .end
  bne .set_last_page
  lda v_temp_page_cur
  cmp #LOW(PAGE_COUNT)
  bcc .end
.set_last_page
  sec
  lda #LOW(PAGE_COUNT)
  sbc #1
  sta v_temp_page_cur
  lda #HIGH(PAGE_COUNT)
  sbc #0
  sta v_temp_page_cur + 1
.end
  rts
