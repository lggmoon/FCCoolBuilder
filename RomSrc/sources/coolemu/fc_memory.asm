PPUCTRL = $2000
PPUMASK = $2001
PPUSTATUS = $2002
OAMADDR = $2003
OAMDATA = $2004
PPUSCROLL = $2005
PPUADDR = $2006
PPUDATA = $2007
OAMDMA = $4014
SQ1VOL = $4000
SQ1SWEEP = $4001
SQ1LO = $4002
SQ1HI = $4003
SQ2VOL = $4004
SQ2SWEEP = $4005
SQ2LO = $4006
SQ2HI = $4007
TRILINEAR = $4008
TRILO = $400A
TRIHI = $400B
NOISEVOL = $400C
NOISELO = $400E
NOISEHI = $400F
DMCFREQ = $4010
DMCRAW = $4011
DMCSTART = $4012
DMCLEN = $4013
APUSTATUS = $4015

PPU_CTRL = $2000
PPU_MASK = $2001
PPU_STATUS = $2002
OAM_ADDR = $2003
OAM_DATA = $2004
PPU_SCROLL = $2005
PPU_ADDR = $2006
PPU_DATA = $2007
OAM_DMA = $4014
SQ1_VOL = $4000
SQ1_SWEEP = $4001
SQ1_LO = $4002
SQ1_HI = $4003
SQ2_VOL = $4004
SQ2_SWEEP = $4005
SQ2_LO = $4006
SQ2_HI = $4007
TRI_LINEAR = $4008
TRI_LO = $400A
TRI_HI = $400B
NOISE_VOL = $400C
NOISE_LO = $400E
NOISE_HI = $400F
DMC_FREQ = $4010
DMC_RAW = $4011
DMC_START = $4012
DMC_LEN = $4013
APU_STATUS = $4015

;-------------------------------------------------------
; 内存分布 寄存器 和 特殊地址
;-------------------------------------------------------
; PPU相关的内存地址
;PPU_CTRL   = $2000
;PPU_MASK   = $2001
;PPU_STATUS = $2002
;PPU_SCROLL = $2005
;PPU_ADDR   = $2006
;PPU_DATA   = $2007

; 精灵
; OAM 64个精灵每个4字节
; 简单的方式是利用$2003 $2004写入OAM，高级的是通过DMA读写精灵OAM 一般利用$0300这个页快速一次写入
; ，如果用dam方式的话，向寄DMA存器 $4014写入3代表把$0300的数据写到oam中
; 1简单模式
;OAM_ADDR   = $2003 ;OAM写入地址
;OAM_DATA   = $2004 ;OAM写入数据
; 2DMA模式
;DMA_ADDR  = $4014 ;DMA写入地址，写入3表示将$0300~-$03ff整页一次写入
SPRITES_DATA  = $0300 ;OMA

; 声音
;APU_CTRL   = $4015  ;APU状态控制

; 控制器
JOY1       = $4016
JOY2       = $4017
JOY2_FRAME = $4017

  ; non-volatile PRG-RAM
;-------------------------------------------------------
; CoolEmu V2 Registers
;-------------------------------------------------------
CE_MODE = $5000

CE_MENU_PRG_BANK = $5001
CE_MENU_CHR_BANK = $5002
CE_MENU_SRAM_PAGE = $5003
CE_MENU_CONFIG = $5004
CE_MENU_DPCM_BANK = $5005
CE_VERSION = $5006
CE_STATUS = $5007

;-------------------------------------------------------
; SRAM layout (menu state)
;-------------------------------------------------------
  .rsset $6000
SRAM_SIGNATURE .rs 8
SRAM_LAST_STARTED_GAME .rs 2
SRAM_LAST_STARTED_LINE .rs 2
SRAM_LAST_STARTED_SAVE .rs 1

;-------------------------------------------------------
; 内存分布 自定义
;-------------------------------------------------------
  .rsset $0000  ; start variables at RAM location 0
COPY_SOURCE_ADDR .rs 2
COPY_DEST_ADDR .rs 2
TMP .rs 2
SELECTED_GAME .rs 2
;
v_temp .rs 2

; 界面相关
v_ppu_addr .rs 2 ; 临时ppu地址
v_screen_point_x .rs 1   ; draw参数 位置 x, max 32
v_screen_point_y .rs 1   ; draw参数 位置 y, max 30
v_screen_offset .rs 2    ; copy参数 页面数据偏移
v_data_len .rs 2       ; copy参数 数据长度
v_data_addr .rs 2      ; copy draw参数 数据地址
v_data_bank .rs 2      ; copy draw参数 bank地址

;菜单相关
v_menu_cur .rs 2
v_menu_cur_save .rs 2
v_page_cur .rs 2
v_temp_page_cur .rs 2
v_temp_menu_cur .rs 2
v_draw_menu_first .rs 2
v_draw_menu_last .rs 2
v_draw_menu_cur .rs 2

;按钮相关
v_buttons        .rs 1  ; 控制器按钮状态
v_buttons_keep   .rs 1
v_buttons_keep_last   .rs 1
v_buttons_temp   .rs 1
v_button_hold_time    .rs 1

;music 相关
FC_Music_DMC_Offset .rs 2

; bank 相关
PRG_BANK .rs 1 ; PRG_A BANK
CHR_BANK .rs 1 ; CHR_A BANK
SRAM_PAGE .rs 1 ; PRG RAM BANK
CART_CONFIG .rs 1 ; variable to store last config
SRAM_PAGES .equ 4 ; number of PRG RAM banks

;save
SAVES .rs 4 ; saves locations
LAST_STARTED_SAVE .rs 1 ; id of last used save

;flash
FLASH_TYPE .rs 1 ; flash memory type

;preloader
CONSOLE_TYPE .rs 1 ; console type

; loader 相关
LOADER_OFFSET_L .rs 1
LOADER_OFFSET_H .rs 1
LOADER_SIZE_L .rs 1
LOADER_SIZE_H .rs 1
LOADER_IDX_L .rs 1
LOADER_IDX_H .rs 1

v_ppu_buffer_count .rs 1
v_attr_value .rs 1
v_attr_buffer_needupdate .rs 1

FC_Music_Prg_8000 .rs 1
FC_Music_Prg_A000 .rs 1
FC_Music_Index_Max .rs 1
FC_Music_Index .rs 1

v_attr_buffer .rs 64
v_ready_go .rs 1

v_mousic_playing .rs 1

v_cursor_time .rs 1
v_cursor_frame .rs 1
v_cursor_bling .rs 1
v_cursor_code .rs 1
v_cursor_x .rs 1
v_cursor_y .rs 1

MusicBSS .rs 1

  .rsset $0100
v_ppu_buffer .rs $ff