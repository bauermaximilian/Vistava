// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public record SourceConfiguration(string ModuleFileName, IDictionary<string, object> Configuration);
