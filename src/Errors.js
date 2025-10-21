// SPDX-License-Identifier: GPL-3.0-or-later

export class InvalidOperationError extends Error { }

export class MissingDependenciesError extends Error { }

export class MissingDotNetDependencyError extends MissingDependenciesError { }