# Uso de Resend en Plan Gratuito y Configuración de Testing

## 1. Manejo de envío de correos con Resend (plan gratuito)

Para evitar errores generados por las restricciones del plan gratuito de
Resend, utilice el siguiente patrón al enviar correos:

``` csharp
bool result = _emailService.SendEmailFunction(string email, any);
if (result)
{
    // Código normal
}
else
{
    // Manejo de error
}
```
Donde **SendEmailFunction(any)** es una función normal de Resend solo que esta configurada para devolver un bool.

``` csharp
public async Task<bool> SendVerificationEmailAsync(string email)
        {
            try
            {
                Log.Information("Iniciando envío de email de verificación a: {Email}", email);
                var htmlBody = await LoadTemplateAsync("AnyEmail");
                var message = new EmailMessage
                {
                    To = email,
                    From = _configuration.GetValue<string>("EmailConfiguration:From")!,
                    Subject = _configuration.GetValue<string>(
                        "EmailConfiguration:AnySubject"
                    )!,
                    HtmlBody = htmlBody,
                };
                var result = await _resend.EmailSendAsync(message);
                if (!result.Success)
                {
                    Log.Error("El envío del email de verificación falló para: {Email}", email);
                    throw new Exception("Error al enviar el correo de verificación.");
                }
                Log.Information("Email de verificación enviado exitosamente a: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al enviar email de verificación a: {Email}", email);
                return false; 
            }
        }
```
Este enfoque permite continuar el flujo de la aplicación sin que los
límites del servicio interrumpan la funcionalidad. Es importante manejar el error de tal forma que no se corte la aplicación. 

Evite usar:

``` csharp
throw new Exception("Error al envier el correo");
```

------------------------------------------------------------------------

## 2. Uso del modo "Testing" para funcionalidades que dependen de correos

Si una funcionalidad **requiere información enviada por email** (por
ejemplo, el código de verificación), puede habilitar el modo *Testing*
en `appsettings.Development.json` para definir manualmente el valor que
debe recibir el sistema.

Ejemplo de implementación:

Par de Key:Value en **appsettings.Development.json**:

``` json
"Testing": {
	"IsTesting": true,
	"FixedVerificationCode": "000000"
}
```

Ejemplo de función para confirmar el correo con un código de verificación:

``` csharp
bool testing = _configuration.GetValue<bool>("Testing:IsTesting"); //Chequeo de variable
CodeType type = CodeType.EmailConfirmation;

var verificationCode = testing
    ? new VerificationCode //Si es que "isTesting es true, se crea un codigo falso
    {
        Code = _configuration.GetValue<string>("Testing:FixedVerificationCode")! ?? "000000",
        CodeType = type,
        GeneralUserId = user.Id,
        Expiration = DateTime.UtcNow.AddHours(1)
    }
    : await _verificationCodeRepository.GetByLatestUserIdAsync( //Si "isTesting" es falso, se ejecuta el codigo normal de la funcionalidad
        user.Id,
        type
    );

if (
    verificationCode.Code != verifyEmailDTO.VerificationCode
    || DateTime.UtcNow >= verificationCode.Expiration
)
{
    // Código normal
}
```

Esto permite simular códigos enviados por correo sin depender realmente
del servicio. 