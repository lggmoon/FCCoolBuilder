
;==============================================================
; NES Menu Example using NESASM3 Assembler
;==============================================================
  ;.inesprg 128 * 1024   ; 1x 16KB PRG code
  .inesprg 8
  .ineschr 0   ; 1x  8KB CHR data
  .inesmap 2   ; mapper 2 = NxROM
  .inesmir 0   ; horizontal mirroring

;--------------------------------------------------------------
; Variables
; Constants
;--------------------------------------------------------------
  .include "fc_memory.asm"

;--------------------------------------------------------------
; auto create build.asm
;--------------------------------------------------------------
  .include "build.asm"


;--------------------------------------------------------------
; 中断向量表
;--------------------------------------------------------------
  .bank 15 ;last bank
  .org $FFFA
  .dw NMI     ; NMI向量
  .dw RESET   ; 重置向量
  .dw IRQ     ; IRQ向量

;--------------------------------------------------------------
; PRG Bank 0 - 程序代码
;--------------------------------------------------------------
  .org $FFE0
unrom_bank_data:
  ; for compatibility with UNROM and UNROM's bus conflicts
  .db $00, $01, $02, $03, $04, $05, $06, $07, $08, $09, $0A, $0B, $0C, $0D, $0E, $0F

  ; routines to be executed from RAM
  .bank 15
  .org $0500 ; actually it's $C500 in cartridge memory
ram_routines:
  .include "fc_banking.asm"
  .include "fc_loader.asm"

  .org $E800
  ;重置向量 - 游戏启动或重置时执行
RESET:
  SEI          ; 禁用IRQ中断
  CLD          ; 禁用十进制模式
  ldx #$40
  stx $4017    ; 禁用APU帧中断
  ldx #$FF
  TXS          ; 设置栈指针
  inx          ; X = 0
  stx PPU_CTRL ; 禁用NMI
  stx PPU_MASK ; 禁用渲染
  stx $4010    ; 禁用DMC IRQ

  ;清空堆栈
  ldx #$FF
  TXS

  ; disable and reset sound
  jsr reset_sound

  ;等待第一个VBlank
  jsr func_wait_for_vblank
  jsr func_wait_for_vblank

  ;清除RAM ($0000-$07FF)
  ldx #$00
  lda #$00
.clrmem:
  sta $0000, X
  sta $0100, X
  sta $0300, X
  sta $0400, X
  sta $0500, X
  sta $0600, X
  sta $0700, X
  inx
  bne .clrmem


  ; loading loader and other RAM routines
  ldx #$00
.load_ram_routines:
  lda ram_routines+$E000, x
  sta ram_routines, x
  lda ram_routines+$E100, x
  sta ram_routines+$100, x
  lda ram_routines+$E200, x
  sta ram_routines+$200, x
  inx
  bne .load_ram_routines

  ; init banks and other cart stuff
  jsr banking_init

  ; detect console type
  jsr console_detect

  ; 清空PPU
  jsr func_clear_ppu_vram

  ; clear sprite data
  jsr clear_sprites
  ; load this empty data
  jsr sprite_dma_copy
  ; loading saved cursor position and other data
  jsr load_state
  jsr func_load_base_chr

  ;等待第二个VBlank
  jsr func_wait_for_vblank

  ; 初始化屏幕
  jsr func_init_screen

    ; 清空刷新计数
  jsr func_init_input

  ; 重置PPU滚动
  lda #$00
  sta PPU_SCROLL
  sta PPU_SCROLL

  ; 启动PPU
  jsr func_wait_for_vblank
  jsr func_start_ppu

  JSR MusicPlay

  ;主循环 - 等待NMI中断
.main_loop:
  jsr MusicUpdate
  jsr func_wait_for_vblank
  jsr func_refresh_cursor
  lda v_ppu_buffer_count
  bne .update_full
  lda v_attr_buffer_needupdate
  bne .update_attr
  jmp .process_input

.update_full
  jsr func_pause_ppu
  lda #BANK(chr_data) / 2 ; bank with CHR
  jsr select_prg_bank
  jsr func_init_ppu_nametable
  jsr func_flush_ppu_buffer
  lda v_attr_buffer_needupdate
  beq .skeep
  jsr func_process_attr_buff
.skeep
  jsr MusicUpdate
  jsr func_wait_for_vblank
  ;jsr func_refresh_cursor
  jsr func_restore_ppu
  jsr func_refresh_cursor
  jmp .process_input

.update_attr
  jsr func_pause_ppu
  jsr func_process_attr_buff
  jsr func_restore_ppu
  jsr func_refresh_cursor
  jmp .process_input

.process_input
  jsr func_read_controller
  jsr func_handle_buttons
  jmp .main_loop

;--------------------------------------------------------------
; NMI中断处理程序 - 每帧执行一次
;--------------------------------------------------------------
NMI:
  rti

IRQ:
  rti


;--------------------------------------------------------------
; include
;--------------------------------------------------------------
  .include "fc_ppu.asm"
  .include "fc_sound.asm"
  .include "fc_screen.asm"
  .include "fc_input.asm"
  .include "fc_preloader.asm"
  .include "fc_save.asm"
  .include "fc_music_player.asm"
