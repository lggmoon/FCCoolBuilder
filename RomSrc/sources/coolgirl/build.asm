; Common constants
GAMES_COUNT = 3
PAGE_MENU_NUM = 14
MENU_TITLE_X = 13
MENU_TITLE_Y = 4
MENU_START_X = 7
MENU_START_Y = 6
MENU_INTER_SPACE = 1
HIGHLIGHT_CODE = 0
CURSOR_OFFSET_X = -2
CURSOR_OFFSET_Y = 0
CURSOR_COLUMN_COUNT = 1
CURSOR_FRAME_COUNT = 1
CURSOR_BLING_COUNT = 2
CURSOR_CODE = 38
CURSOR_SPEED = 4
ZERO_CODE = 2
DOT_CODE = 1
CURSOR_REFRESH_TIME = 5
ENABLE_LAST_GAME_SAVING = 1
ENABLE_SYSTEM_SOUND = 0

 .bank 13
 .org $A000
FAMISTUDIO_MUSIC_DATA
 .include "../../musics/WQ1506_Series.asm"

 .bank 14
 .org $D000
FAMISTUDIO_SFX_DATA
 .include "../../sfxes/sfx_400in1.asm"

; CHR Data
 .bank 12
 .org $8000
chr_data:
 .incbin "../../themes/pic14/0.chr"
ppu_palette_data:
 .incbin "../../themes/pic14/0.col"
background_nametable_data:
 .incbin "../../themes/pic14/0.nam"
background_attrtable_data:
 .incbin "../../themes/pic14/0.atb"

 .bank 15
 .org $FF00
menu_foot:
  .db $29, $0E, $1A, $1A, $17, $12, $14, $1D, $17, $00

menu_title:
  .db $05, $29, $14, $19, $29, $03, $00


; Loader data

  .bank 0
  .org $8000
loader_data_reg_0:
  .db $00, $00, $00

  .bank 0
  .org $8100
loader_data_reg_1:
  .db $08, $20, $10

  .bank 0
  .org $8200
loader_data_reg_2:
  .db $F8, $F8, $F0

  .bank 0
  .org $8300
loader_data_reg_3:
  .db $00, $00, $80

  .bank 0
  .org $8400
loader_data_reg_4:
  .db $10, $10, $F0

  .bank 0
  .org $8500
loader_data_reg_5:
  .db $01, $01, $01

  .bank 0
  .org $8600
loader_data_reg_6:
  .db $10, $10, $0F

  .bank 0
  .org $8700
loader_data_reg_7:
  .db $89, $81, $89

  .bank 0
  .org $8800
loader_data_chr_start_bank_h:
  .db $00, $00, $00

  .bank 0
  .org $8900
loader_data_chr_start_bank_l:
  .db $28, $30, $38

  .bank 0
  .org $8A00
loader_data_chr_start_bank_s:
  .db $80, $80, $80

  .bank 0
  .org $8B00
loader_data_chr_count:
  .db $10, $10, $10

  .bank 0
  .org $8C00
loader_data_game_save:
  .db $00, $00, $00

  .bank 0
  .org $8D00
loader_data_game_flags:
  .db $00, $00, $00

  .bank 0
  .org $8E00
loader_data_cursor_pos:
  .db $12, $12, $1C

; Game names

  .bank 0
  .org $9000
game_names:
  .dw game_name_0
  .dw game_name_1
  .dw game_name_2


  .bank 1
  .org $A000
; Snow Bros. (U) [!].nes
game_name_0:
  .db $03, $01, $1E, $19, $1A, $22, $29, $0D, $1D, $1A, $1E, $01, $29, $29, $20
  .db $29, $29, $29, $29, $29, $00

; Snow Bros. (E) [!].nes
game_name_1:
  .db $04, $01, $1E, $19, $1A, $22, $29, $0D, $1D, $1A, $1E, $01, $29, $29, $10
  .db $29, $29, $29, $29, $29, $00

; Castlevania III - Cadence of Agony by Dr. Mario+Optomon v20091228 (Hack).nes
game_name_2:
  .db $05, $01, $0E, $0C, $1E, $1F, $17, $10, $21, $0C, $19, $14, $0C, $29, $14
  .db $14, $14, $29, $29, $29, $0E, $0C, $0F, $10, $19, $0E, $10, $01, $01, $01
  .db $00
