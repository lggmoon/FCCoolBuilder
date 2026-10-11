save_state:
  ; saving last started game
  jsr enable_sram
  lda #0 ; first SRAM bank
  jsr select_sram_page
  ; storing signature
  ldx #0
.signature_loop:
  lda saves_signature, x
  sta SRAM_SIGNATURE, x
  inx
  cpx #8
  bne .signature_loop
  ; storing selected game
  lda <SELECTED_GAME
  sta SRAM_LAST_STARTED_GAME
  lda <SELECTED_GAME+1
  sta SRAM_LAST_STARTED_GAME+1

  ; storing save ID of last started game
  lda <LAST_STARTED_SAVE
  sta SRAM_LAST_STARTED_SAVE
  jsr disable_sram
  rts

load_state:
  ; loading saved state
  jsr enable_sram
  lda #0 ; first SRAM bank
  jsr select_sram_page
  ; check for signature
  ldx #0
.signature_loop:
  lda saves_signature, x
  cmp SRAM_SIGNATURE, x
  bne .end
  inx
  cpx #8
  bne .signature_loop
  ; loading last started game
  .ifdef ENABLE_LAST_GAME_SAVING
    lda SRAM_LAST_STARTED_GAME
    sta <SELECTED_GAME
    lda SRAM_LAST_STARTED_GAME+1
    sta <SELECTED_GAME+1
    ; check for invalid value
    lda <SELECTED_GAME
    sec
    sbc #LOW(GAMES_COUNT)
    lda <SELECTED_GAME+1
    sbc #HIGH(GAMES_COUNT)
    bcs .ovf
  .else
    lda #0
    sta <SELECTED_GAME
    sta <SELECTED_GAME+1
  .endif
  ; loading last save ID
  lda SRAM_LAST_STARTED_SAVE
  sta <LAST_STARTED_SAVE
  jmp .end
.ovf:
  ; reset values
  lda #0
  sta <SELECTED_GAME
  sta <SELECTED_GAME+1
.end:
  jsr disable_sram
  rts
saves_signature:
  .db 'C','O','O','L','S','A','V','E'
