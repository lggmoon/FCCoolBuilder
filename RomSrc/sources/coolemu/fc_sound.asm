func_button_sound:
  .if FAMISTUDIO_SFX_DATA >0
    jsr PlayMusicSfx0
  .else
    .if ENABLE_SYSTEM_SOUND = 1
      lda #01
      sta APUSTATUS
      lda #$87
      sta SQ1VOL
      lda #$89
      sta SQ1SWEEP
      lda #$f0
      sta SQ1LO
      lda #$00
      sta SQ1HI
    .endif
    .if ENABLE_SYSTEM_SOUND = 2
      lda #$08
      sta APUSTATUS
      lda #$01
      sta NOISEVOL
      lda #$0C
      sta NOISELO
      lda #$01
      sta NOISEHI
    .endif
  .endif
  rts

func_button_alt_sound:
  .if FAMISTUDIO_SFX_DATA >0
    jsr PlayMusicSfx1
  .else
    .if ENABLE_SYSTEM_SOUND = 1
      ora #01
      sta APUSTATUS
      lda #$3f
      sta SQ1VOL
      lda #$9a
      sta SQ1SWEEP
      lda #$ff
      sta SQ1LO
      lda #$00
      sta SQ1HI
    .endif
  .endif
  rts

  ; game start sound
start_sound:
  ;enable channel
  lda #%00000001
  sta APUSTATUS
  ;square 1
  lda #%00011111
  sta SQ1VOL
  ; sweep
  lda #%10011010
  sta SQ1SWEEP
  ; timer
  lda #%11111111
  sta SQ1LO
  ; length counter and timer
  lda #%10010000
  sta SQ1HI
  rts

  ; error sound
error_sound:
  .if FAMISTUDIO_SFX_DATA >0
    jsr PlayMusicSfx3
  .else
    .if ENABLE_SYSTEM_SOUND = 1
      ; enable channel
      lda #%00000100
      sta APUSTATUS
      ; triangle
      lda #%01001111
      sta TRILINEAR
      ; timer
      lda #%00000000
      sta TRILO
      ; length counter and timer
      lda #%11110011
      sta TRIHI
    .endif
  .endif
  rts

wait_sound_end:
  lda APUSTATUS
  bne wait_sound_end
  rts

reset_sound:
  lda #0
  sta SQ1_VOL
  sta SQ1_SWEEP
  sta SQ1_LO
  sta SQ1_HI
  sta SQ2_VOL
  sta SQ2_SWEEP
  sta SQ2_LO
  sta SQ2_HI
  sta TRILINEAR
  sta TRI_LO
  sta TRI_HI
  sta NOISE_VOL
  sta NOISE_LO
  sta NOISE_HI
  sta DMC_FREQ
  sta DMC_RAW
  sta DMC_START
  sta DMC_LEN
  ldx #$40
  stx JOY2_FRAME ; disable APU frame IRQ
  rts
