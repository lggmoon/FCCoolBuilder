;--------------------------------------------------------------
; Constants
;--------------------------------------------------------------
; 按钮位掩码
BUTTON_HOLD_TIME = 18
BUTTON_HOLD_SPEED = CURSOR_SPEED

; 按钮位掩码
BUTTON_A      = %10000000
BUTTON_B      = %01000000
BUTTON_SELECT = %00100000
BUTTON_START  = %00010000
BUTTON_UP     = %00001000
BUTTON_DOWN   = %00000100
BUTTON_LEFT   = %00000010
BUTTON_RIGHT  = %00000001

LOGI_BUTTON_PAGE_PREV = BUTTON_LEFT
LOGI_BUTTON_PAGE_NEXT = BUTTON_RIGHT
LOGI_BUTTON_START = BUTTON_SELECT | BUTTON_START

;--------------------------------------------------------------
; 处理input
;--------------------------------------------------------------
func_init_input:
  rts
;--------------------------------------------------------------
; 处理input
;--------------------------------------------------------------
func_read_controller:
  jsr func_read_buttons ; first read
  lda v_buttons_keep
  sta v_buttons_temp
  jsr func_read_buttons ; second read
  lda v_buttons_keep
  cmp v_buttons_temp
  beq .checked ; 确定按了一次
  lda v_buttons_keep_last
  sta v_buttons_keep
.checked
  ;和上一次对比，不同的
  eor v_buttons_keep_last
  and v_buttons_keep
  sta v_buttons ; 真正获得一次
  ;保存上一次kepp
  lda v_buttons_keep
  sta v_buttons_keep_last
  jsr func_button_delay
  rts

;--------------------------------------------------------------
; 读取按键buttons
;--------------------------------------------------------------
func_read_buttons:
  lda #1
  sta JOY1
  lda #0
  sta JOY1
  ldy #8
.read_button:
  lda JOY1
  lsr A
  rol v_buttons_keep
  dey
  bne .read_button
  rts

func_button_delay:                ;按键长按延迟
 lda v_buttons_keep
 beq .delay_end
 cmp #$10
 beq .delay_end
 lda v_button_hold_time
 cmp #BUTTON_HOLD_TIME
 BCS .pressed
 INC v_button_hold_time
 rts
.pressed:
 INC v_button_hold_time
 lda v_button_hold_time
 SEC
 SBC #BUTTON_HOLD_TIME
 cmp #BUTTON_HOLD_SPEED
 BCS .dealy_finish
 rts
.dealy_finish:
 lda v_buttons_keep
 sta v_buttons
 lda #BUTTON_HOLD_TIME
 sta v_button_hold_time
.delay_end:
 sta v_button_hold_time
 rts


  ;--------------------------------------------------------------
; 处理菜单输入
;--------------------------------------------------------------
func_handle_buttons:
  lda v_buttons
  cmp #$00
  bne .check_up
  rts

.check_up
  lda v_buttons
  AND #BUTTON_UP
  beq .check_down
  jsr func_menu_cursor_up
  rts

.check_down:
  lda v_buttons
  AND #BUTTON_DOWN
  beq .check_left
  jsr func_menu_cursor_down
  rts

.check_left:
  lda v_buttons
  AND #LOGI_BUTTON_PAGE_PREV
  beq .check_right
  jsr func_menu_page_prev
  rts

.check_right:
  lda v_buttons
  AND #LOGI_BUTTON_PAGE_NEXT
  beq .check_start
  jsr func_menu_page_next
  rts

.check_start:
  lda v_buttons
  AND #LOGI_BUTTON_START
  beq .end
  jsr start_game
  rts
.end
  lda #$0
  sta v_buttons
  rts

func_menu_cur_changed:
  lda v_menu_cur + 1
  cmp #HIGH(GAMES_COUNT)
  bcc .check_page_change
  bne .set_menu_cur_last_one
  lda v_menu_cur
  cmp #LOW(GAMES_COUNT)
  bcc .check_page_change
.set_menu_cur_last_one
  lda #LOW(GAMES_COUNT - 1)
  sta v_menu_cur
  lda #HIGH(GAMES_COUNT - 1)
  sta v_menu_cur + 1
.check_page_change
  jsr func_count_page_cur_to_temp_page_cur
  lda v_temp_page_cur + 1
  cmp v_page_cur + 1
  bne .refresh
  lda v_temp_page_cur
  cmp v_page_cur
  beq .end
.refresh
  lda v_temp_page_cur + 1
  sta v_page_cur + 1
  lda v_temp_page_cur
  sta v_page_cur
  jsr func_update_screen
  rts
.end
  jsr func_update_cursor
  rts

func_menu_cursor_up:
  lda v_menu_cur + 1
  sta v_menu_cur_save + 1
  lda v_menu_cur
  sta v_menu_cur_save

  lda v_menu_cur + 1
  bne .dec_menu_cur
  lda v_menu_cur
  bne .dec_menu_cur
  ;cur = 0
  lda #LOW(GAMES_COUNT-1)
  sta v_menu_cur
  lda #HIGH(GAMES_COUNT-1)
  sta v_menu_cur + 1
  jmp .end
.dec_menu_cur
  sec
  lda v_menu_cur
  sbc #$1
  sta v_menu_cur
  lda v_menu_cur + 1
  sbc #$0
  sta v_menu_cur + 1
.end
  jsr func_menu_cur_changed
  jsr func_button_sound
  rts

func_menu_cursor_down:
  lda v_menu_cur + 1
  sta v_menu_cur_save + 1
  lda v_menu_cur
  sta v_menu_cur_save

  clc
  lda v_menu_cur
  adc #$1
  sta v_menu_cur
  lda v_menu_cur +1
  adc #$0
  sta v_menu_cur +1
  cmp #HIGH(GAMES_COUNT)
  bcc .end
  bne .set_zore
  lda v_menu_cur
  cmp #LOW(GAMES_COUNT)
  bcc .end
.set_zore
  lda #0
  sta v_menu_cur
  sta v_menu_cur+1
.end
  jsr func_menu_cur_changed
  jsr func_button_sound
  rts

func_menu_page_prev:
  lda v_menu_cur + 1
  sta v_menu_cur_save + 1
  lda v_menu_cur
  sta v_menu_cur_save

  lda #HIGH(PAGE_COUNT)
  bne .check_page_0
  lda #LOW(PAGE_COUNT)
  cmp #2
  bcs .check_page_0

  lda #0
  sta v_menu_cur
  sta v_menu_cur+1
  jmp .end

.check_page_0
  ;是否是0页
  lda v_page_cur + 1
  bne .do
  lda v_page_cur
  bne .do

  ;设置到最后一页
  clc
  lda v_menu_cur
  adc #LOW((PAGE_COUNT-1)*PAGE_MENU_NUM)
  sta v_menu_cur
  lda v_menu_cur + 1
  adc #HIGH((PAGE_COUNT-1)*PAGE_MENU_NUM)
  sta v_menu_cur + 1
  jmp .end

.do
  sec
  lda v_menu_cur
  sbc #LOW(PAGE_MENU_NUM)
  sta v_menu_cur
  lda v_menu_cur + 1
  sbc #HIGH(PAGE_MENU_NUM)
  sta v_menu_cur + 1

.end
  jsr func_menu_cur_changed
  jsr func_button_alt_sound
  rts

func_menu_page_next:
  lda v_menu_cur + 1
  sta v_menu_cur_save + 1
  lda v_menu_cur
  sta v_menu_cur_save

  lda #HIGH(PAGE_COUNT)
  bne .check_page_0
  lda #LOW(PAGE_COUNT)
  cmp #2
  bcs .check_page_0

  lda #HIGH(GAMES_COUNT)
  bne .set_last_menu
  lda #LOW(GAMES_COUNT)
  bne .set_last_menu
  rts

.set_last_menu
  sec
  lda #LOW(GAMES_COUNT)
  sbc #1
  sta v_menu_cur
  lda #HIGH(GAMES_COUNT)
  sbc #0
  sta v_menu_cur + 1
  jmp .end

.check_page_0
  ;是否是最后页
  lda v_page_cur + 1
  cmp #HIGH(PAGE_COUNT-1)
  bcc .do
  lda v_page_cur
  cmp #LOW(PAGE_COUNT-1)
  bcc .do

  ;设置到第0页
  sec
  lda v_menu_cur
  sbc #LOW((PAGE_COUNT-1)*PAGE_MENU_NUM)
  sta v_menu_cur
  lda v_menu_cur + 1
  sbc #HIGH((PAGE_COUNT-1)*PAGE_MENU_NUM)
  sta v_menu_cur + 1
  jmp .end

.do
  clc
  lda v_menu_cur
  adc #LOW(PAGE_MENU_NUM)
  sta v_menu_cur
  lda v_menu_cur +1
  adc #HIGH(PAGE_MENU_NUM)
  sta v_menu_cur +1

.end
  jsr func_menu_cur_changed
  jsr func_button_alt_sound
  rts
