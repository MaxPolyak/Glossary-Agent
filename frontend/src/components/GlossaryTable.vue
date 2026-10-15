<script setup lang="ts">
import { ref } from 'vue'
import type { Term } from '../api'

defineProps<{
  terms: Term[]
  // названия терминов, которые появились после последнего запроса — их подсвечиваем
  newNames: string[]
  loading: boolean
}>()

const emit = defineEmits<{
  refresh: []
  reset: []
}>()

const confirmOpen = ref(false)

function confirmReset() {
  confirmOpen.value = false
  emit('reset')
}
</script>

<template>
  <v-card>
    <!-- не v-card-title: внутри него кнопки наследуют крупный шрифт заголовка -->
    <div class="d-flex align-center px-4 pt-3">
      <span class="text-h6">Глоссарий</span>
      <v-chip class="ml-2" size="small">{{ terms.length }}</v-chip>

      <v-spacer />

      <v-btn
        variant="text"
        prepend-icon="mdi-refresh"
        :disabled="loading"
        @click="emit('refresh')"
      >
        Обновить
      </v-btn>

      <v-btn
        variant="text"
        color="error"
        prepend-icon="mdi-delete-outline"
        :disabled="loading || terms.length === 0"
        @click="confirmOpen = true"
      >
        Сбросить
      </v-btn>
    </div>

    <v-card-text>
      <p v-if="terms.length === 0" class="text-medium-emphasis text-center py-8">
        Пока пусто. Отправьте текст с определениями — термины появятся здесь.
      </p>

      <v-table v-else>
        <thead>
          <tr>
            <th class="glossary__name-column">Термин</th>
            <th>Определение</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="term in terms"
            :key="term.name"
            :class="{ 'bg-yellow-lighten-5': newNames.includes(term.name) }"
          >
            <td class="font-weight-medium">
              {{ term.name }}
              <v-chip v-if="newNames.includes(term.name)" class="ml-1" color="success" size="x-small">
                новый
              </v-chip>
            </td>
            <td>{{ term.definition }}</td>
          </tr>
        </tbody>
      </v-table>
    </v-card-text>

    <v-dialog v-model="confirmOpen" max-width="420">
      <v-card>
        <v-card-title>Сбросить глоссарий?</v-card-title>
        <v-card-text>
          Все термины ({{ terms.length }}) будут удалены из базы. Вернуть их можно, только отправив тексты заново.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="confirmOpen = false">Отмена</v-btn>
          <v-btn color="error" variant="flat" @click="confirmReset">Удалить всё</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-card>
</template>

<style scoped>
.glossary__name-column {
  width: 30%;
}
</style>
