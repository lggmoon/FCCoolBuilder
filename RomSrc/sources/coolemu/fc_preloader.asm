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
  ;lda loader_data_game_flags, x
  ;and CONSOLE_TYPE
  ;beq compatible_console
  jmp compatible_console

  ; not compatible console!
  ; error sound
  jsr error_sound
  ; save state, without game save
  lda #0
  sta <LAST_STARTED_SAVE
  jsr save_state
  jmp RESET

compatible_console:
  jsr clear_screen
  jsr clear_sprites
  jsr sprite_dma_copy

  ldx <SELECTED_GAME

  lda loader_data_game_offset_l, x
  sta <LOADER_OFFSET_L
  lda loader_data_game_offset_h, x
  sta <LOADER_OFFSET_H
  lda loader_data_game_size_l, x
  sta <LOADER_SIZE_L
  lda loader_data_game_size_h, x
  sta <LOADER_SIZE_H
  lda loader_data_game_idx_l, x
  sta <LOADER_IDX_L
  lda loader_data_game_idx_h, x
  sta <LOADER_IDX_H

  jsr save_state
  ; start loader stored into RAM
  ; first PRG bank
  lda #0
  jsr select_prg_bank

  jsr wait_sound_end
  jsr reset_sound
  ; start loader stored into RAM
  jmp loader

