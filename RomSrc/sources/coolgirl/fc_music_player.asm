  .if (FAMISTUDIO_MUSIC_DATA + FAMISTUDIO_SFX_DATA) !=0
; FamiStudio config.
FAMISTUDIO_CFG_EXTERNAL       = 1
FAMISTUDIO_CFG_DPCM_SUPPORT   = 1
FAMISTUDIO_CFG_SFX_SUPPORT    = 1
FAMISTUDIO_CFG_SFX_STREAMS    = 2
FAMISTUDIO_CFG_EQUALIZER      = 1
FAMISTUDIO_USE_VOLUME_TRACK   = 1
FAMISTUDIO_USE_PITCH_TRACK    = 1
FAMISTUDIO_USE_SLIDE_NOTES    = 1
FAMISTUDIO_USE_VIBRATO        = 1
FAMISTUDIO_USE_ARPEGGIO       = 1
FAMISTUDIO_CFG_SMOOTH_VIBRATO = 1
FAMISTUDIO_USE_RELEASE_NOTES  = 1
FAMISTUDIO_USE_DELTA_COUNTER  = 1
FAMISTUDIO_DPCM_OFF           = $A000

; NESASM-specific config.
FAMISTUDIO_NESASM_ZP_RSSET  = MusicBSS
FAMISTUDIO_NESASM_BSS_RSSET = $400
FAMISTUDIO_NESASM_CODE_BANK = 14
FAMISTUDIO_NESASM_CODE_ORG  = $C000
  .include "famistudio_nesasm.asm"
  .endif
;  .bank 13
;  .org $A000
;BACKGROUND_MUSIC_DATA
  ;.include "song_silver_surfer_nesasm.asm"
  ;.include "song_shatterhand_nesasm.asm"
  ;.include "superc.asm"
  ;.include "sfx_nesasm.asm"
  ;.bank 14
  ;.include "bb.asm"

  .bank 15
MusicPlay
  .if FAMISTUDIO_MUSIC_DATA > 0
    lda #BANK(FAMISTUDIO_MUSIC_DATA) / 2
    jsr select_prg_bank
    lda #1
    ldx #LOW(FAMISTUDIO_MUSIC_DATA)
    ldy #HIGH(FAMISTUDIO_MUSIC_DATA)
    jsr famistudio_init
    lda #0
    jsr famistudio_music_play
  .endif
  .if FAMISTUDIO_SFX_DATA > 0
    .if FAMISTUDIO_MUSIC_DATA = 0
      lda #1
      ldx #$00
      ldy #$80
      jsr famistudio_init
    .endif
    ldx #LOW(FAMISTUDIO_SFX_DATA)
    ldy #HIGH(FAMISTUDIO_SFX_DATA)
    jsr famistudio_sfx_init
  .endif
  lda #1
  sta v_mousic_playing
  rts

MusicUpdate
  lda v_mousic_playing
  beq .end
  .if FAMISTUDIO_MUSIC_DATA > 0
    lda #BANK(FAMISTUDIO_MUSIC_DATA) / 2
    jsr select_prg_bank
  .endif
  .if (FAMISTUDIO_MUSIC_DATA + FAMISTUDIO_SFX_DATA) != 0
    jsr famistudio_update
  .endif
.end
  rts

MusicStopForStartGame
  .if FAMISTUDIO_MUSIC_DATA > 0
    lda #BANK(FAMISTUDIO_MUSIC_DATA) / 2
    jsr select_prg_bank
  .endif
  .if (FAMISTUDIO_MUSIC_DATA + FAMISTUDIO_SFX_DATA) != 0
    .if FAMISTUDIO_SFX_DATA > 0
      jsr famistudio_music_pause
      jsr PlayMusicSfx2
      ldx #30
.wait_sound_end:
      jsr func_wait_for_vblank
      txa
      pha
      jsr famistudio_update
      pla
      tax
      lda APUSTATUS
      beq .stop
      dex
      bne .wait_sound_end
.stop
    .endif
    jsr famistudio_music_stop
    lda #%00000000
    sta APUSTATUS
    .if ENABLE_SYSTEM_SOUND > 0
      jsr start_sound
    .endif
  .else
    .if ENABLE_SYSTEM_SOUND > 0
      jsr start_sound
    .endif
  .endif
  lda #0
  sta v_mousic_playing
  rts

PlayMusicSfx0:
  .if FAMISTUDIO_SFX_DATA >0
    lda #1
    jsr PlayMusicSfx
  .endif
  rts

PlayMusicSfx1:
  .if FAMISTUDIO_SFX_DATA >0
    lda FAMISTUDIO_SFX_DATA
    cmp FAMISTUDIO_SFX_DATA +1
    bne .lda0
    lda #1
    jmp .play
.lda0
    lda #0
.play
    jsr PlayMusicSfx
  .endif
  rts

PlayMusicSfx2
  .if FAMISTUDIO_SFX_DATA >0
    lda FAMISTUDIO_SFX_DATA
    cmp FAMISTUDIO_SFX_DATA +1
    bne .lda0
    lda FAMISTUDIO_SFX_DATA +1
    cmp FAMISTUDIO_SFX_DATA +2
    bne .lda1
    lda #2
    jmp .play
.lda0
    lda #0
    jmp .play
.lda1
    lda #1
    jmp .play
.play
    jsr PlayMusicSfx
  .endif
  rts

PlayMusicSfx3
  .if FAMISTUDIO_SFX_DATA >0
    lda FAMISTUDIO_SFX_DATA
    cmp FAMISTUDIO_SFX_DATA +1
    bne .lda0
    lda FAMISTUDIO_SFX_DATA +1
    cmp FAMISTUDIO_SFX_DATA +2
    bne .lda1
    lda FAMISTUDIO_SFX_DATA +2
    cmp FAMISTUDIO_SFX_DATA +3
    bne .lda2
    lda #2
    jmp .play
.lda0
    lda #0
    jmp .play
.lda1
    lda #1
    jmp .play
.lda2
    lda #2
    jmp .play
.play
    jsr PlayMusicSfx
  .endif
  rts

PlayMusicSfx:
  .if FAMISTUDIO_SFX_DATA >0
    ldx #FAMISTUDIO_SFX_CH0
    jsr famistudio_sfx_play
  .endif
  rts
