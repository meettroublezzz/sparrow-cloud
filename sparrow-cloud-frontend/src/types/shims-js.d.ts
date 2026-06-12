// // 全局声明：匹配项目中所有 .js 模块
// declare module '*.js' {
//   // 将 JS 文件导出内容强制标记为 any
//   const content: any
//   export default content
//   // 如果 JS 里有命名导出，补充这一行（兼容更多写法）
//   export * as all from '*.js'
// }