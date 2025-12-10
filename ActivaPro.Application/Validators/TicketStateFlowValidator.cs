using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivaPro.Application.Validators
{
    /// <summary>
    /// Validador del flujo de estados de tickets
    /// Flujo: Pendiente → Asignado → En Proceso → Resuelto → Cerrado
    /// </summary>
    public static class TicketStateFlowValidator
    {
        // Flujo de estados permitido
        private static readonly Dictionary<string, List<string>> FlujosPermitidos = new()
        {
            { "Pendiente", new List<string> { "Asignado" } },
            { "Asignado", new List<string> { "En Proceso", "Pendiente" } }, // Puede retroceder a Pendiente
            { "En Proceso", new List<string> { "Resuelto", "Asignado" } }, // Puede retroceder a Asignado
            { "Resuelto", new List<string> { "Cerrado", "En Proceso" } }, // Puede retroceder a En Proceso
            { "Cerrado", new List<string>() } // Estado final, no puede cambiar
        };

        // Estados que requieren técnico asignado
        private static readonly List<string> EstadosQueRequierenTecnico = new()
        {
            "Asignado", "En Proceso", "Resuelto"
        };

        /// <summary>
        /// Valida si la transición de estado es permitida
        /// </summary>
        public static (bool EsValida, string MensajeError) ValidarTransicion(
            string estadoActual,
            string estadoNuevo,
            int? idTecnicoAsignado,
            string comentario)
        {
            // VALIDACIÓN 1: Comentario obligatorio
            if (string.IsNullOrWhiteSpace(comentario))
            {
                return (false, "⚠️ Debe proporcionar un comentario obligatorio que justifique el cambio de estado.");
            }

            if (comentario.Length < 10)
            {
                return (false, "⚠️ El comentario debe tener al menos 10 caracteres.");
            }

            // VALIDACIÓN 2: Estados válidos
            if (!FlujosPermitidos.ContainsKey(estadoActual))
            {
                return (false, $"❌ Estado actual '{estadoActual}' no es válido.");
            }

            // VALIDACIÓN 3: No se puede cambiar desde Cerrado
            if (estadoActual == "Cerrado")
            {
                return (false, "🔒 No se puede cambiar el estado de un ticket cerrado.");
            }

            // VALIDACIÓN 4: El estado no puede ser el mismo
            if (estadoActual == estadoNuevo)
            {
                return (false, $"ℹ️ El ticket ya está en estado '{estadoNuevo}'.");
            }

            // VALIDACIÓN 5: Transición permitida en el flujo
            var transicionesPermitidas = FlujosPermitidos[estadoActual];
            if (!transicionesPermitidas.Contains(estadoNuevo))
            {
                var siguientesEstados = string.Join(", ", transicionesPermitidas);
                return (false,
                    $"❌ No se puede cambiar de '{estadoActual}' a '{estadoNuevo}'. " +
                    $"Los estados permitidos son: {siguientesEstados}");
            }

            // VALIDACIÓN 6: Técnico asignado requerido
            if (EstadosQueRequierenTecnico.Contains(estadoNuevo) && !idTecnicoAsignado.HasValue)
            {
                return (false,
                    $"⚠️ No se puede cambiar a '{estadoNuevo}' sin un técnico asignado. " +
                    "Por favor, asigne un técnico primero.");
            }

            // ✅ Transición válida
            return (true, string.Empty);
        }

        /// <summary>
        /// Obtiene los estados permitidos desde el estado actual
        /// </summary>
        public static List<string> ObtenerEstadosPermitidos(string estadoActual)
        {
            if (FlujosPermitidos.ContainsKey(estadoActual))
            {
                return FlujosPermitidos[estadoActual];
            }

            return new List<string>();
        }

        /// <summary>
        /// Verifica si un estado requiere técnico asignado
        /// </summary>
        public static bool RequiereTecnicoAsignado(string estado)
        {
            return EstadosQueRequierenTecnico.Contains(estado);
        }

        /// <summary>
        /// Obtiene el siguiente estado lógico en el flujo
        /// </summary>
        public static string ObtenerSiguienteEstado(string estadoActual)
        {
            return estadoActual switch
            {
                "Pendiente" => "Asignado",
                "Asignado" => "En Proceso",
                "En Proceso" => "Resuelto",
                "Resuelto" => "Cerrado",
                _ => estadoActual
            };
        }

        /// <summary>
        /// Obtiene el emoji representativo del estado
        /// </summary>
        public static string ObtenerEmojiEstado(string estado)
        {
            return estado switch
            {
                "Pendiente" => "⏳",
                "Asignado" => "👤",
                "En Proceso" => "⚙️",
                "Resuelto" => "✅",
                "Cerrado" => "🔒",
                _ => "❓"
            };
        }

        /// <summary>
        /// Obtiene el color de badge para el estado
        /// </summary>
        public static string ObtenerColorEstado(string estado)
        {
            return estado switch
            {
                "Pendiente" => "warning",
                "Asignado" => "info",
                "En Proceso" => "primary",
                "Resuelto" => "success",
                "Cerrado" => "dark",
                _ => "secondary"
            };
        }

        /// <summary>
        /// Obtiene todos los estados posibles
        /// </summary>
        public static List<string> ObtenerTodosLosEstados()
        {
            return new List<string>
            {
                "Pendiente",
                "Asignado",
                "En Proceso",
                "Resuelto",
                "Cerrado"
            };
        }

        /// <summary>
        /// Genera un mensaje descriptivo de la transición
        /// </summary>
        public static string GenerarMensajeTransicion(string estadoAnterior, string estadoNuevo)
        {
            var emojiAnterior = ObtenerEmojiEstado(estadoAnterior);
            var emojiNuevo = ObtenerEmojiEstado(estadoNuevo);

            return $"{emojiAnterior} '{estadoAnterior}' → {emojiNuevo} '{estadoNuevo}'";
        }
    }
}