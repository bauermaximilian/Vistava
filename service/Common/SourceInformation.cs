// SPDX-License-Identifier: GPL-3.0-or-later

namespace Vistava.Service.Common;

public record SourceInformation(IDictionary<string, SourceConfiguration> Sources, List<string> Errors);