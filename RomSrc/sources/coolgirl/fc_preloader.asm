; starting game!
start_game:
  ;
  lda v_menu_cur
  sta SELECTED_GAME
  lda v_menu_cur + 1
  sta SELECTED_GAME + 1
  ; disable PPU
  lda #%00000000
  sta PPUCTRL
  lda #%00000000
  sta PPUMASK

  ; stop music
  jsr MusicStopForStartGame

  ; wait for v-blank
  jsr waitblank_simple

  lda <SELECTED_GAME + 1
  jsr select_prg_bank
  ldx <SELECTED_GAME
  lda loader_data_game_flags, x
  and CONSOLE_TYPE
  beq compatible_console

  ; not compatible console!
  ; error sound
  jsr error_sound
  ; save state, without game save
  lda #0
  sta <LAST_STARTED_SAVE
  jsr save_state
  jmp RESET

compatible_console:
  ; clear NTRAM
  jsr enable_chr_write
  jsr enable_four_screen
  jsr clear_screen
  ; clear nametables
  jsr disable_four_screen
  jsr clear_screen
  jsr disable_chr_write
  ; clear sprite data
  jsr clear_sprites
  ; load this empty data
  jsr sprite_dma_copy

  ; loading game settings
  ldx <SELECTED_GAME
  lda loader_data_reg_0, x
  sta <LOADER_REG_0
  lda loader_data_reg_1, x
  sta <LOADER_REG_1
  lda loader_data_reg_2, x
  sta <LOADER_REG_2
  lda loader_data_reg_3, x
  sta <LOADER_REG_3
  lda loader_data_reg_4, x
  sta <LOADER_REG_4
  lda loader_data_reg_5, x
  sta <LOADER_REG_5
  lda loader_data_reg_6, x
  sta <LOADER_REG_6
  lda loader_data_reg_7, x
  sta <LOADER_REG_7
  lda loader_data_chr_start_bank_h, x
  sta <LOADER_CHR_START_H
  lda loader_data_chr_start_bank_l, x
  sta <LOADER_CHR_START_L
  lda loader_data_chr_start_bank_s, x
  sta <LOADER_CHR_START_S
  lda loader_data_chr_count, x
  sta <LOADER_CHR_LEFT
  lda loader_data_game_save, x
  sta <LOADER_GAME_SAVE
  sta <LAST_STARTED_SAVE ; save ID of save
  lda #2
  sta <LOADER_GAME_SAVE_BANK
  ; loading battery backed save if need
  jsr load_save
  ; saving state
  jsr save_state
  ; start loader stored into RAM
  ; first PRG bank
  lda #0
  jsr select_prg_bank
  ; loading tiles
  jsr load_all_chr_banks
  ; wait for sound end and reset sound registers
  jsr wait_sound_end
  jsr reset_sound
  ; start loader stored into RAM
  jmp loader

