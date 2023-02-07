namespace BUTR.NativeAOT.Generator;

public static class HeaderStrings
{
    public const string CImportHeaders = @"
#include <stdlib.h>
#include <stdint.h>
";
    public const string CppImportHeaders = @"
#include <memory>
#include <string>
#include <cstdint>
#include <cuchar>
";
    public const string Types = @"
#ifndef __cplusplus
        typedef char16_t wchar_t;
#endif
        typedef char16_t param_string;
        typedef char16_t param_json;
        typedef uint8_t param_data;
        typedef uint8_t param_bool;
        typedef int32_t param_int;
        typedef uint32_t param_uint;
        typedef void param_ptr;

        typedef struct return_value_void
        {
            param_string *const error;
        } return_value_void;
        typedef struct return_value_string
        {
            param_string *const error;
            param_string *const value;
        } return_value_string;
        typedef struct return_value_json
        {
            param_string *const error;
            param_json *const value;
        } return_value_json;
        typedef struct return_value_data
        {
            param_string *const error;
            param_data *const value;
            param_int length;
        } return_value_data;
        typedef struct return_value_bool
        {
            param_string *const error;
            param_bool const value;
        } return_value_bool;
        typedef struct return_value_int32
        {
            param_string *const error;
            param_int const value;
        } return_value_int32;
        typedef struct return_value_uint32
        {
            param_string *const error;
            param_uint const value;
        } return_value_uint32;
        typedef struct return_value_ptr
        {
            param_string *const error;
            param_ptr *const value;
        } return_value_ptr;
";
}