<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { clearGlossary, extractTerms, fetchGlossary, type Term } from './api'
import ExtractForm from './components/ExtractForm.vue'
import GlossaryTable from './components/GlossaryTable.vue'

type Status = { kind: 'success' | 'error'; text: string }

const terms = ref<Term[]>([])
const newNames = ref<string[]>([])
const loading = ref(false)
const status = ref<Status | null>(null)

async function loadGlossary() {
  try {
    terms.value = await fetchGlossary()
  } catch (error) {
    status.value = { kind: 'error', text: getMessage(error) }
  }
}

async function onSubmit(text: string) {
  loading.value = true
  status.value = null
  newNames.value = []

  const namesBefore = terms.value.map((term) => term.name)
  const startedAt = Date.now()

  try {
    await extractTerms(text)
    await loadGlossary()

    // Бэкенд по ТЗ отвечает просто 200, без списка терминов.
    // Поэтому что добавилось, считаем сами: сравниваем глоссарий до и после.
    newNames.value = terms.value
      .map((term) => term.name)
      .filter((name) => !namesBefore.includes(name))

    const seconds = Math.round((Date.now() - startedAt) / 1000)
    status.value = {
      kind: 'success',
      text: `200 OK за ${seconds} с. Новых терминов: ${newNames.value.length}`,
    }
  } catch (error) {
    status.value = { kind: 'error', text: `500: ${getMessage(error)}` }
  } finally {
    loading.value = false
  }
}

async function onReset() {
  loading.value = true
  status.value = null
  newNames.value = []

  try {
    const deleted = await clearGlossary()
    await loadGlossary()
    status.value = { kind: 'success', text: `Глоссарий очищен. Удалено терминов: ${deleted}` }
  } catch (error) {
    status.value = { kind: 'error', text: getMessage(error) }
  } finally {
    loading.value = false
  }
}

function getMessage(error: unknown): string {
  return error instanceof Error ? error.message : String(error)
}

onMounted(loadGlossary)
</script>

<template>
  <v-app>
    <v-app-bar color="primary" density="comfortable">
      <v-app-bar-title>Glossary Extraction Agent</v-app-bar-title>
    </v-app-bar>

    <v-main class="bg-grey-lighten-4">
      <v-container class="page">
        <p class="text-medium-emphasis mb-4">
          Вставьте текст документа — агент найдёт в нём определения и расшифровки и добавит их в глоссарий.
        </p>

        <v-row>
          <v-col cols="12" md="6">
            <ExtractForm :loading="loading" @submit="onSubmit" />

            <v-alert v-if="status" class="mt-4" :type="status.kind" variant="tonal">
              {{ status.text }}
            </v-alert>
          </v-col>

          <v-col cols="12" md="6">
            <GlossaryTable
              :terms="terms"
              :new-names="newNames"
              :loading="loading"
              @refresh="loadGlossary"
              @reset="onReset"
            />
          </v-col>
        </v-row>
      </v-container>
    </v-main>
  </v-app>
</template>

<style scoped>
.page {
  max-width: 1280px;
}
</style>
