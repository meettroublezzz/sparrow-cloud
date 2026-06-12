import { defineStore } from 'pinia'
import type { StoreDefinition } from 'pinia'

export const useUserStore: StoreDefinition = defineStore('user', {
  state: () => ({
    userId: 'test',
  }),
})

export default useUserStore